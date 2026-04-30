# ASP.NET Core Application Testing

In this tutorial, you will learn how to integration-test an ASP.NET Core Web API using FEFF.TestFixtures. By the end, you will be able to:

- Spin up a real test application in-memory.
- Override configuration values before the app starts.
- Control time and randomness for deterministic tests.
- Isolate database state across tests with temporary database names.
- Verify HTTP responses and database side-effects together.

## Prerequisites

- .NET 8.0 or later
- xUnit v3 (the examples use xUnit v3, but the same fixtures work with TUnit)
- A running PostgreSQL instance (or container) if you plan to execute the database examples
- Basic familiarity with ASP.NET Core Minimal APIs and EF Core

## The Application Under Test

The examples in this tutorial target a small Web API with the following endpoints:

| Endpoint | Description |
|----------|-------------|
| `GET /weatherforecast/const` | Returns a hard-coded weather forecast. |
| `GET /weatherforecast/env` | Returns a forecast whose `summary` comes from config (`summary` key). |
| `GET /weatherforecast/time` | Returns a forecast whose `date` comes from `TimeProvider`. |
| `GET /weatherforecast/random` | Returns a forecast whose `temperatureC` comes from `Random`. |
| `POST /user` | Creates a `User` record in the database. |

Create an application skeleton in `program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace WebApiTestSubject;

public class Program
{
    public const string ConnectionStringName = "PgDb";

    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Register services here
        // ...
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // app.Map ...

        app.Run();
    }
}
```

The application registers `Random`, `TimeProvider`, and an EF Core `DbContext` in its service container. The full subject application is available in the [project repo](https://github.com/metacoder-feff/FEFF.TestFixtures/tree/main/tests/Subjects/WebApiTestSubject).

## Step 1 — Install Packages

Add the following packages to your test project:

```bash
dotnet add package FEFF.TestFixtures.XunitV3
dotnet add package FEFF.TestFixtures.AspNetCore
dotnet add package FEFF.TestFixtures.AspNetCore.EF
```

Optional but recommended for fluent assertions:

```bash
dotnet add package AwesomeAssertions
dotnet add package AwesomeAssertions.Json
```

## Step 2 — Register the Extension

In any C# file in your test project, add the assembly-level attribute so that xUnit v3 loads the FEFF.TestFixtures engine:

```csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

## Step 3 — Configure Database Isolation

When multiple tests run in parallel, they must not collide on the same database. `TmpDatabaseNameFixture` redirects configured connection strings to unique temporary database names. To tell it which connection strings to rewrite, create an options fixture:

```csharp
using FEFF.TestFixtures.AspNetCore.EF;

[Fixture]
public class OptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => [Program.ConnectionStringName];
}
```

> **Tip:** The connection string name (`"PgDb"` here) must match the name used by the application under test.

## Step 4 — Compose the Fixture Set

Instead of inheriting from a base test class, compose all the infrastructure you need into a single `record`. Each fixture is injected automatically by the engine:

```csharp
[Fixture]
public record FixtureSet(
    AppManagerFixture<Program> AppManagerFx,
    FakeRandomFixture<Program> FakeRandomFx,
    FakeTimeFixture<Program> FakeTimeFx,
    AppClientFixture<Program> ClientFx,
    DatabaseLifecycleFixture<Program, ApplicationDbContext> DbFx,
    TmpDatabaseNameFixture<Program, OptionsFixture> TmpDbNameFx
);
```

| Fixture | Purpose |
|---------|---------|
| `AppManagerFixture<Program>` | Configures and manages the `TestApplication` lifecycle. |
| `FakeRandomFixture<Program>` | Replaces the app's `Random` with a deterministic fake. |
| `FakeTimeFixture<Program>` | Replaces the app's `TimeProvider` with a controllable fake. |
| `AppClientFixture<Program>` | Provides an `HttpClient` pointing at the test application. |
| `DatabaseLifecycleFixture<Program, ApplicationDbContext>` | Ensures the database is created and later deleted. |
| `TmpDatabaseNameFixture<Program, OptionsFixture>` | Rewrites the connection string to a unique temporary name. |

## Step 5 — Create the Test Class

Retrieve the `FixtureSet` from the test context and expose convenience properties so your tests stay concise:

```csharp
public class ApiTests
{
    protected FixtureSet FixtureSet { get; } =
        TestContext.Current.GetFeffFixture<FixtureSet>();

    protected FakeRandom AppRandom => 
        FixtureSet.FakeRandomFx.Value;

    protected FakeTimeProvider AppTime => 
        FixtureSet.FakeTimeFx.Value;

    protected IAppConfigurator AppConfigurationBuilder => 
        FixtureSet.AppManagerFx.ConfigurationBuilder;

    protected IDatabaseLifecycleFixture DbFx => 
        FixtureSet.DbFx;

    protected HttpClient Client => 
        FixtureSet.ClientFx.LazyValue;

    protected ApplicationDbContext AppDbCtx => 
        FixtureSet.DbFx.LazyDbContext;

    // Tests will go here...
}
```

> **Lazy initialization:** The application is built and started on the first access to `AppManagerFx.LazyApplication` directly via other fixtures. This means you can still configure the app via `AppConfigurationBuilder` *before* it is started (e.g. the first HTTP request).

## Step 6 — Create and Test a Basic Endpoint

Add application endpoint:
```csharp
// Program.cs
//...
app.MapGet("/weatherforecast/const", () =>
{
    return new List<WeatherForecast>()
    {
        new (DateOnly.Parse("2000-01-01"), 20, "normal")
    };
});
```

Verify that the constant endpoint returns the expected JSON:

```csharp
// Tests.cs
[Fact]
public async Task Example1__Client__should__get_response()
{
    var resp = await Client.GetAsync("/weatherforecast/const", TestContext.Current.CancellationToken);
    var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

    JToken.Parse(body)
        .Should().BeEquivalentTo(
        """
        [
            {
                "date": "2000-01-01",
                "temperatureC": 20,
                "summary": "normal"
            }
        ]
        """);
}
```

## Step 7 — Override Configuration Before Startup
Add application endpoint:
```csharp
// Program.cs
//...
app.MapGet("/weatherforecast/env", (IConfiguration c) =>
{
    var summary = c.GetValue<string>("summary");
    return new List<WeatherForecast>()
    {
        new (DateOnly.Parse("2000-01-01"), 20, summary)
    };
});
```

Use `AppConfigurationBuilder.UseSetting` to inject configuration values before the application starts. Because the app is lazily initialized, the setting is applied in time:

```csharp
// Tests.cs
[Theory]
[InlineData("cloudy")]
[InlineData("warm")]
public async Task Example2__SetEnvVar__should_make_api_to_respond_with(string envVarValue)
{
    AppConfigurationBuilder.UseSetting("summary", envVarValue);

    var resp = await Client.GetAsync("/weatherforecast/env", TestContext.Current.CancellationToken);
    var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

    JToken.Parse(body)
        .Should().BeEquivalentTo(
        $$"""
        [
            {
                "date": "2000-01-01",
                "temperatureC": 20,
                "summary": "{{envVarValue}}"
            }
        ]
        """);
}
```

## Step 8 — Control Time

Add Services and application endpoint:
```csharp
// Program.cs
//...
builder.Services.AddSingleton((_) => TimeProvider.System)
//...
app.MapGet("/weatherforecast/time", (TimeProvider t) =>
{
    var now = t.GetUtcNow();
    return new List<WeatherForecast>()
    {
        new (DateOnly.FromDateTime(now.Date) , 20, "normal")
    };
});
```

`FakeTimeFixture` replaces the application's `TimeProvider` with `FakeTimeProvider`. You can set the exact instant returned by `GetUtcNow()`:

```csharp
// Tests.cs
[Theory]
[InlineData("2006-01-05")]
[InlineData("2150-11-15")]
public async Task Example3__FakeTimeFixture__should_make_api_to_respond__with(string date)
{
    AppTime.SetUtcNow(DateTimeOffset.Parse($"{date}T05:05:05Z"));

    var resp = await Client.GetAsync("/weatherforecast/time", TestContext.Current.CancellationToken);
    var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

    JToken.Parse(body)
        .Should().BeEquivalentTo(
        $$"""
        [
            {
                "date": "{{date}}",
                "temperatureC": 20,
                "summary": "normal"
            }
        ]
        """);
}
```

## Step 9 — Control Randomness

Add Services and application endpoint:
```csharp
// Program.cs
//...
builder.Services.AddSingleton((_) => Random.Shared))
//...
app.MapGet("/weatherforecast/random", (Random r) =>
{
    var t = r.Next(100);
    return new List<WeatherForecast>()
    {
        new (DateOnly.Parse("2000-01-01"), t, "normal")
    };
});
```

`FakeRandomFixture` replaces the application's `Random` with a `FakeRandom` instance. Assign a fixed strategy so `Random.Next()` always returns the value you expect:

```csharp
// Tests.cs
[Theory]
[InlineData(42)]
[InlineData(77)]
public async Task Example4__FakeRandomFixture__should_make_api_to_respond__with(int temperature)
{
    AppRandom.Int32Next = FixedNextStrategy.From(temperature);

    var resp = await Client.GetAsync("/weatherforecast/random", TestContext.Current.CancellationToken);
    var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

    JToken.Parse(body)
        .Should().BeEquivalentTo(
        $$"""
        [
            {
                "date": "2000-01-01",
                "temperatureC": {{temperature}},
                "summary": "normal"
            }
        ]
        """);
}
```

## Step 10 — Verify Database Side-Effects

Create DbContext:
```csharp
// ApplicationContext.cs
public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }
}
```

Add DbContext and application endpoint:
```csharp
// Program.cs
//...
builder.Services..AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var connStr = sp
        .GetRequiredService<IConfiguration>()
        .GetConnectionString(ConnectionStringName)
        ;
    options.UseNpgsql(connStr);
});
//...
app.MapPost("/user", async (ApplicationDbContext dbCtx) =>
{
    dbCtx.Users.Add(new User()
    {
        Age = 100,
        Name = "test",
    });
    await dbCtx.SaveChangesAsync();
});
```

Integration tests should confirm that data is actually persisted. `DatabaseLifecycleFixture` can create the database on demand, and `LazyDbContext` gives you direct access to EF Core for assertions:

```csharp
// Tests.cs
[Fact]
public async Task Example5__Post_user__should_create_record_in_db()
{
    await DbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

    var resp = await Client.PostAsync("/user", null, TestContext.Current.CancellationToken);
    var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

    var users = await AppDbCtx.Users.ToListAsync(TestContext.Current.CancellationToken);

    JToken.FromObject(users)
        .Should().BeEquivalentTo("""
        [
            {
                "Id": 1,
                "Name": "test",
                "Age": 100
            }
        ]
        """);
}
```

> **Cleanup:** `DatabaseLifecycleFixture` automatically deletes the temporary database after the test finishes, so the next test starts from a clean slate.

### Entire application services registration section

```csharp
// Program.cs
//...
builder.Services
    .AddSingleton((_) => Random.Shared)
    .AddSingleton((_) => TimeProvider.System)
    .AddScoped<SomeService>()
    .AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        var connStr = sp
            .GetRequiredService<IConfiguration>()
            .GetConnectionString(ConnectionStringName)
            ;
        options.UseNpgsql(connStr);
    });
//...
```

## Summary

You have now written integration tests that:

1. **Host the application in-memory** using `AppManagerFixture` and `AppClientFixture`.
2. **Request the application**  via HTTP client
3. **Inject configuration** before startup via `IAppConfigurator`.
4. **Control externalities** like time and randomness with `FakeTimeFixture` and `FakeRandomFixture`.
5. **Isolate data** across tests using `TmpDatabaseNameFixture`.
6. **Assert on persistence** by combining HTTP calls with direct `DbContext` queries.

## Next Steps

- Explore the full list of ASP.NET Core fixtures in the [Fixtures reference](../fixtures/asp-net-core/toc.yml).
- Learn how to [create your own fixtures](../01.getting-started/creating-custom-fixtures.md) for domain-specific test infrastructure.
