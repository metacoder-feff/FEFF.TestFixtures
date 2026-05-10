# ASP.NET Core Application Testing

In this tutorial, you will learn how to integration-test an ASP.NET Core Web API using FEFF.TestFixtures. By the end, you will be able to:

- Spin up a real test application in-memory.
- Override configuration values before the app starts.
- Control time and randomness for deterministic tests.
- Isolate database state across tests with temporary database names.
- Verify HTTP responses and database side effects together.

## Prerequisites

- .NET 8.0 or later
- xUnit v3 (the tutorial uses xUnit v3, but the same fixtures work with TUnit)
- A running PostgreSQL instance
- Basic familiarity with ASP.NET Core minimal APIs and Entity Framework Core

## The Application Under Test

The tutorial assumes an ASP.NET Core application with the following endpoints:

| Endpoint | Description |
|----------|-------------|
| `POST /weatherforecast/generate` | Creates a weather forecast using the system's `TimeProvider`, `Random`, and `IConfiguration`, then persists it to the database via EF Core. |
| `GET /weatherforecast/today` | Returns the persisted weather forecast for the current date. |

The application registers `TimeProvider`, `Random`, and `ApplicationDbContext` in its dependency injection container:

```csharp
builder.Services
    .AddSingleton((_) => Random.Shared)
    .AddSingleton((_) => TimeProvider.System)
    .AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        var connStr = sp.GetRequiredService<IConfiguration>()
            .GetConnectionString("PgDb");
        options.UseNpgsql(connStr);
    });
```

<details>
  <summary>Expand to see full Program.cs source</summary>

The complete `Program.cs` file used in this tutorial, including `ApplicationDbContext`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace WebApiTestSubject;

public class Program
{
    public const string ConnectionStringName = "PgDb";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services
            .AddSingleton((_) => Random.Shared)
            .AddSingleton((_) => TimeProvider.System)
            .AddScoped<SomeService>()
            .AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var connStr = builder.Configuration.GetConnectionString(ConnectionStringName);
                options.UseNpgsql(connStr);
            });

        var app = builder.Build();

        app.MapPost("/weatherforecast/generate", async (TimeProvider tp, Random r, IConfiguration cfg, ApplicationDbContext dbCtx) =>
        {
            var now = tp.GetUtcNow();
            var date = DateOnly.FromDateTime(now.Date);
            var temperature = r.Next(100);
            var summary = cfg.GetValue<string>("summary");

            var forecast = new WeatherForecast(date, temperature, summary);
            dbCtx.WeatherForecasts.Add(new WeatherForecastEntity { Data = forecast });
            
            await dbCtx.SaveChangesAsync();
        });

        app.MapGet("/weatherforecast/today", async (TimeProvider tp, ApplicationDbContext dbCtx) =>
        {
            var now = tp.GetUtcNow();
            var today = DateOnly.FromDateTime(now.Date);

            var entity = await dbCtx.WeatherForecasts
                .Where(x => x.Data.Date == today)
                .FirstOrDefaultAsync();
                
            return entity is null ? Results.NotFound() : Results.Ok(entity.Data);
        });

        app.Run();
    }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<WeatherForecastEntity> WeatherForecasts { get; init; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecastEntity>().ComplexProperty(e => e.Data);
    }
}

public class WeatherForecastEntity
{
    public long Id { get; init; }
    public required WeatherForecast Data { get; init; }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
```

</details>

---


## Step 1: Install the Required Packages

Add the following packages to your test project:

```bash
dotnet add package AwesomeAssertions
dotnet add package AwesomeAssertions.Json
dotnet add package FEFF.TestFixtures.XunitV3
dotnet add package FEFF.TestFixtures.AspNetCore
dotnet add package FEFF.TestFixtures.AspNetCore.EF
```

> [!NOTE]
> `AwesomeAssertions` and `AwesomeAssertions.Json` are used in this tutorial for readable assertions and JSON equivalence checking. You can use any assertion library you prefer.

## Step 2: Enable the Test Fixtures Extension

Register the FEFF.TestFixtures extension with xUnit v3 by adding the assembly-level attribute to any file in your test project:

```csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

## Step 3: Configure Database Isolation

To prevent tests from interfering with one another, use [`TmpDatabaseNameFixture`](../fixtures/asp-net-core/TmpDatabaseNameFixture.md) to redirect the application's connection string to a unique temporary database for each test scope.

First, define an options fixture that tells `TmpDatabaseNameFixture` which connection strings to redirect:

```csharp
using FEFF.TestFixtures;
using FEFF.TestFixtures.AspNetCore;

[Fixture]
public class OptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => ["PgDb"];
}
```

This fixture is requested automatically by `TmpDatabaseNameFixture` and ensures every test runs against its own database.

> **Tip:** The connection string name (`"PgDb"` here) must match the name used by the application under test.

## Step 4: Compose the Fixture Set

FEFF.TestFixtures encourages composition over inheritance. Instead of deriving from a base class, bundle all the fixtures your tests need into a single `FixtureSet` record:

```csharp
using FEFF.TestFixtures;
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.EF;
using FEFF.TestFixtures.AspNetCore.Randomness;
using Microsoft.Extensions.Time.Testing;

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

Each fixture serves a specific purpose:

| Fixture | Purpose |
|---------|---------|
| `AppManagerFixture<Program>` | Manages the test application lifecycle and pre-startup configuration. |
| `FakeRandomFixture<Program>` | Replaces the app's `Random` with a deterministic fake. |
| `FakeTimeFixture<Program>` | Replaces the app's `TimeProvider` with a controllable fake. |
| `AppClientFixture<Program>` | Provides an `HttpClient` wired to the test application. |
| `DatabaseLifecycleFixture<Program, ApplicationDbContext>` | Ensures the database is created and cleaned up after the test. |
| `TmpDatabaseNameFixture<Program, OptionsFixture>` | Redirects the connection string to a unique temporary database. |

## Step 5: Create the Test Class

Retrieve the `FixtureSet` in your test class and expose convenience properties for the parts you access most often:

```csharp
using AwesomeAssertions;
using AwesomeAssertions.Json;
using FEFF.TestFixtures;
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.EF;
using FEFF.TestFixtures.AspNetCore.Randomness;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using Xunit.v3;

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

    // ... tests will go here
}
```

These properties reduce noise in your test methods and make the intent of each test clearer.

> **Lazy initialization:** The application is built and started on the first access to `AppManagerFx.LazyApplication`, either directly or via other fixtures. This means you can still configure the app via `AppConfigurationBuilder` *before* it starts (e.g., before the first HTTP request).

## Step 6: Write the Integration Test

Now write the test that exercises the full flow. The test configures the application's environment, triggers forecast generation, verifies database persistence, and validates the HTTP response.

```csharp
[Fact]
public async Task Generate_weatherforecast__should_persist_and_return()
{
    // Arrange
    var expectedDate = "2025-06-15";
    var expectedTemperature = 42;
    var expectedSummary = "sunny";

    // Control what TimeProvider.GetUtcNow() returns
    AppTime.SetUtcNow(DateTimeOffset.Parse($"{expectedDate}T12:00:00Z"));

    // Control what Random.Next() returns
    AppRandom.Int32Next = FixedNextStrategy.From(expectedTemperature);

    // Inject a configuration value before the application starts
    AppConfigurationBuilder.UseSetting("summary", expectedSummary);

    // Ensure the isolated database is created and migrated
    // Note: The application starts here
    await DbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

    // Act: trigger forecast generation
    await PostAsync(Client, "/weatherforecast/generate", null);

    // Assert: verify the forecast was persisted to the database
    var forecastEntities = await AppDbCtx.WeatherForecasts
        .ToListAsync(TestContext.Current.CancellationToken);

    var forecasts = forecastEntities.Select(x => x.Data).ToList();

    JToken.FromObject(forecasts)
        .Should().BeEquivalentTo($$"""
        [
            {
                "Date": "{{expectedDate}}",
                "TemperatureC": {{expectedTemperature}},
                "Summary": "{{expectedSummary}}",
            }
        ]
        """);


    // Act: retrieve the forecast via the API
    var response = await GetAsync(Client, "/weatherforecast/today");

    // Assert: verify the API returns the persisted forecast
    response
        .Should().BeEquivalentTo(
        $$"""
        {
            "date": "{{expectedDate}}",
            "temperatureC": {{expectedTemperature}},
            "summary": "{{expectedSummary}}"
        }
        """);
}
```

<details>
  <summary>Expand to see full ApiTests.cs source</summary>

The complete `ApiTests.cs` file used in this tutorial:

```csharp
using System.Net;
using AwesomeAssertions;
using AwesomeAssertions.Json;
using FEFF.TestFixtures;
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.Randomness;
using FEFF.TestFixtures.AspNetCore.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebApiTestSubject;
using Xunit.v3;

// Register the FEFF TestFixtures extension with xUnit v3
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]

namespace ExampleTests.AspNetCore;

[Fixture]
public class OptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => [Program.ConnectionStringName];
}

[Fixture]
public record FixtureSet(
    AppManagerFixture<Program> AppManagerFx,
    FakeRandomFixture<Program> FakeRandomFx,
    FakeTimeFixture<Program> FakeTimeFx,provider
    AppClientFixture<Program> ClientFx,
    DatabaseLifecycleFixture<Program, ApplicationDbContext> DbFx,
    TmpDatabaseNameFixture<Program, OptionsFixture> TmpDbNameFx
);

public class ApiTests
{
    protected FixtureSet FixtureSet { get; } = TestContext.Current.GetFeffFixture<FixtureSet>();

    #region properties for fast access

    protected FakeRandom AppRandom => FixtureSet.FakeRandomFx.Value;
    protected FakeTimeProvider AppTime => FixtureSet.FakeTimeFx.Value;
    protected IAppConfigurator AppConfigurationBuilder => FixtureSet.AppManagerFx.ConfigurationBuilder;
    protected IDatabaseLifecycleFixture DbFx => FixtureSet.DbFx;
    protected HttpClient Client => FixtureSet.ClientFx.LazyValue;
    protected ApplicationDbContext AppDbCtx => FixtureSet.DbFx.LazyDbContext;
    #endregion

    [Fact]
    public async Task Example_Tutorial_Asp__Api__should_persist_and_return()
    {
        var expectedDate = "2025-06-15";
        var expectedTemperature = 42;
        var expectedSummary = "sunny";

        AppTime.SetUtcNow(DateTimeOffset.Parse($"{expectedDate}T12:00:00Z"));
        AppRandom.Int32Next = FixedNextStrategy.From(expectedTemperature);
        // This should be set before the app starts
        AppConfigurationBuilder.UseSetting("summary", expectedSummary);

        // Start Application and then create the database
        await DbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await PostAsync(Client, "/weatherforecast/generate", null);

        var forecastEntities = await AppDbCtx.WeatherForecasts.ToListAsync(TestContext.Current.CancellationToken);
        var forecasts = forecastEntities.Select(x => x.Data).ToList();
        // Assert the WeatherForecasts table contains exactly one record with expected properties
        JToken.FromObject(forecasts)
            .Should().BeEquivalentTo($$"""
            [
                {
                    "Date": "{{expectedDate}}",
                    "TemperatureC": {{expectedTemperature}},
                    "Summary": "{{expectedSummary}}",
                }
            ]
            """);

        var response = await GetAsync(Client, "/weatherforecast/today");

        response
            .Should().BeEquivalentTo(
            $$"""
            {
                "date": "{{expectedDate}}",
                "temperatureC": {{expectedTemperature}},
                "summary": "{{expectedSummary}}"
            }
            """);
    }

    # region helpers

    private static async Task<JToken> GetAsync(HttpClient client, string url)
    {
        var getResp = await client.GetAsync(url, TestContext.Current.CancellationToken);
        var getBody = await getResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        getResp.StatusCode.Should().Be(HttpStatusCode.OK, getBody);
        return JToken.Parse(getBody);
    }

    private static async Task PostAsync(HttpClient client, string url, string? data)
    {
        StringContent? sc = null;
        if(data != null)
            sc = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            
        var resp = await client.PostAsync(url, sc, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);
    }
    #endregion
}
```
</details>

---

### What the test does

1. **Deterministic setup**
   - [`FakeTimeFixture`](../fixtures/asp-net-core/FakeTimeFixture.md) pins the system clock to `2025-06-15T12:00:00Z`.
   - [`FakeRandomFixture`](../fixtures/asp-net-core/FakeRandomFixture.md) forces `Random.Next()` to return `42`.
   - [`AppManagerFixture`](../fixtures/asp-net-core/AppManagerFixture.md) injects the `"summary"` setting before the application starts.

2. **Database preparation**
   - [`DatabaseLifecycleFixture`](../fixtures/asp-net-core-ef/DatabaseLifecycleFixture.md) creates the database schema. Because [`TmpDatabaseNameFixture`](../fixtures/asp-net-core/TmpDatabaseNameFixture.md) is also in the fixture set, the schema is created inside a uniquely named temporary database.

3. **HTTP interaction**
   - [`AppClientFixture`](../fixtures/asp-net-core/AppClientFixture.md) provides the `HttpClient`.
   - The `POST` endpoint uses the faked services to generate the forecast and saves it via EF Core.

4. **Dual verification**
   - The test queries `AppDbCtx.WeatherForecasts` directly to prove the data was actually persisted.
   - The test then calls `GET /weatherforecast/today` to prove the API returns the same data.

## Step 7: Run the Test

Execute the test from the command line:

```bash
dotnet test --filter "FullyQualifiedName~ApiTests"
```

The test should pass in a few seconds. Because all external factors (time, randomness, database name) are controlled by fixtures, the result is deterministic and repeatable.

## How It Works

### Lazy initialization

Fixtures are created on first access. The ASP.NET Core application is not started until you either:

- Make an HTTP request through `AppClientFixture.LazyValue`, or
- Call `DbFx.EnsureCreatedAsync()`.

This allows you to configure the application (settings, faked services) before it boots.

### Automatic cleanup

When the test finishes, the fixture scope is disposed:

- The temporary database is deleted by `DatabaseLifecycleFixture`.
- The test application is shut down.
- The `HttpClient` is disposed.

You do not need any `[CollectionDefinition]` or `IClassFixture` boilerplate; FEFF.TestFixtures manages the lifecycle for you.

### Test isolation

`TmpDatabaseNameFixture` intercepts the connection string during application startup and appends a unique suffix. Even if multiple tests run in parallel, each operates on a separate database, eliminating cross-test contamination.

## Summary

You have now written integration tests that:

1. **Host the application in-memory** using `AppManagerFixture` and `AppClientFixture`.
2. **Request the application** via an HTTP client.
3. **Inject configuration** before startup via `IAppConfigurator`.
4. **Control externalities** like time and randomness with `FakeTimeFixture` and `FakeRandomFixture`.
5. **Isolate data** across tests using `TmpDatabaseNameFixture` and `DatabaseLifecycleFixture`.
6. **Assert on persistence** by combining HTTP calls with direct `DbContext` queries.

## See Also

| Reference | Description |
|-----------|-------------|
| [Program.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/Subjects/WebApiTestSubject/Program.cs) | The application under test used in this tutorial. |
| [ApiTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | The complete integration test implementation. |
| [Fixture List](../fixtures/list.md) | Explore fixtures |
| [Creating Custom Fixtures](../getting-started/creating-custom-fixtures.md) | Learn how to create your own fixtures for domain-specific test infrastructure. |
