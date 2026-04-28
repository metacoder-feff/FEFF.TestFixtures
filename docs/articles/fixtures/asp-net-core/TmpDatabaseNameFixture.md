# TmpDatabaseNameFixture

**Assembly**: `FEFF.TestFixtures.AspNetCore.dll`  
**Namespace**: `FEFF.TestFixtures.AspNetCore`  
**Source**: [TmpDatabaseNameFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/TmpDatabaseNameFixture.cs)

## Overview

The `TmpDatabaseNameFixture` is an extension to [`AppManagerFixture`](AppManagerFixture.md). It appends a unique test identifier to specified connection string database names, ensuring each test runs against an isolated database. This prevents test interference and enables parallel test execution.

### Key Features

- **Unique Database per Test**: Ensures complete database isolation between tests
- **Configurable Connection Strings**: Can patch single or multiple connection strings
- **Automatic Registration**: Seamlessly integrates with the application's configuration
- **Database Provider Agnostic**: Does not require Entity Framework Core or any specific database provider—only patches connection strings

## Basic Usage

### Step 0: Define Your Test Application

First, define your test application entry point with Entity Framework Core configured.  
This example uses EF Core to access the database, but the fixture does not require it:

```csharp
using Microsoft.EntityFrameworkCore;

namespace WebApiTestSubject;

public class Program
{
    public const string ConnectionStringName = "PgDb";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Register DbContext with connection string
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connStr = sp
                .GetRequiredService<IConfiguration>()
                .GetConnectionString(ConnectionStringName);
            options.UseNpgsql(connStr);
        });
        
        // Other service registrations...
        
        var app = builder.Build();

        // Define endpoints...
        app.MapGet("/db-info", async (ApplicationDbContext dbCtx) =>
        {
            // See also: 'DatabaseLifecycleFixture' to automate DB creation in tests
            await dbCtx.Database.EnsureCreatedAsync();
            return new
            {
                Name = dbCtx.Database.GetDbConnection().Database
            };
        });

        app.Run();
    }
}
```

**Important**: The connection string should be defined in your configuration (e.g., `appsettings.json`, environment variables):

```json
{
  "ConnectionStrings": {
    "PgDb": "Server=localhost;Database=MyAppDb;Username=test;Password=test;"
  }
}
```

### Step 1: Define the Options Interface

First, create an options fixture that implements `ITmpDatabaseNameFixtureOptions`:

```csharp
using FEFF.TestFixtures.AspNetCore;

[Fixture]
public class OptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => 
        [Program.ConnectionStringName];
}
```

### Step 2: Use the Fixture in Your Tests

```csharp
using FEFF.TestFixtures.AspNetCore;
using Microsoft.EntityFrameworkCore;

public class DatabaseTests
{
    protected IAppClientFixture ClientFx { get; } = 
        TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();

    // Fixture instance is not used explicitly in tests:
    // Database name is patched during fixture construction
    protected TmpDatabaseNameFixture<Program, OptionsFixture> TmpDbNameFx { get; } = 
        TestContext.Current.GetFeffFixture<TmpDatabaseNameFixture<Program, OptionsFixture>>();

    [Fact]
    public async Task Fixture__should_make_api_respond_with_replaced_db_name()
    {
        var resp = await ClientFx.LazyValue.GetAsync("/db-info", TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var name = JToken.Parse(body)["name"]?.Value<string>();

        // By default, the DB name is configured as "MyAppDb"
        name.Should()
            .NotBe("MyAppDb")
            .And
            .NotBeNullOrEmpty();
    }
}
```

## How It Works

The `TmpDatabaseNameFixture` automatically:

1. **Generates a Unique Identifier**: Uses the test scope ID to create a unique suffix (e.g., `test-guid123`)
2. **Modifies Connection Strings**: Appends the suffix to the `Database` property of specified connection strings
3. **Applies at Configuration Time**: Makes changes during application startup before any database access

### Example Transformation

Original connection string:
```
Server=localhost;Database=MyAppDb;Trusted_Connection=True;
```

After the fixture is applied:
```
Server=localhost;Database=MyAppDb-test-abc123;Trusted_Connection=True;
```

## Key Members

All functionality is implemented in the constructor; there are no public members to access directly.

## Advanced Scenarios

### Multiple Databases

For applications using multiple databases, define all connection strings in your options:

```csharp
[Fixture]
public class MultiDbOptions : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => 
        [Program.MainConnectionStringName, Program.AuditConnectionStringName];
}

// Use in tests
TmpDatabaseNameFixture<Program, MultiDbOptions> MultiDbFx = 
    TestContext.Current.GetFeffFixture<TmpDatabaseNameFixture<Program, MultiDbOptions>>();
```

### Combining with DatabaseLifecycleFixture

For complete database management, combine with `DatabaseLifecycleFixture`:

```csharp
[Fixture]
public record FixtureSet(
    AppClientFixture<Program> Client,
    TmpDatabaseNameFixture<Program, OptionsFixture> TmpDbName,
    DatabaseLifecycleFixture<Program, ApplicationDbContext> DbLifecycle
);
```

## See Also

| Link | Description |
|------|-------------|
| [API: TmpDatabaseNameFixture](xref:FEFF.TestFixtures.AspNetCore.TmpDatabaseNameFixture`2) | API reference |
| [TmpDatabaseNameFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/TmpDatabaseNameFixtureTests.cs) | Unit tests |
| [DatabaseLifecycleFixture](../asp-net-core-ef/DatabaseLifecycleFixture.md) | Database lifecycle management |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Example usage in API tests |
