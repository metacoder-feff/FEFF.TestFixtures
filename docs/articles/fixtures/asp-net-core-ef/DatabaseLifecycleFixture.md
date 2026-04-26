# DatabaseLifecycleFixture

**Assembly**: `FEFF.TestFixtures.AspNetCore.EF.dll`  
**Namespace**: `FEFF.TestFixtures.AspNetCore.EF`  
**Source**: [DatabaseLifecycleFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore.EF/DatabaseLifecycleFixture.cs)

## Overview

The `DatabaseLifecycleFixture` is an extension to [`AppServicesFixture`](../asp-net-core/AppServicesFixture.md).   
It manages Entity Framework Core database creation and deletion for the lifetime of a test scope.

This fixture ensures that the database exists and is properly migrated before tests, and automatically cleans up the database after tests complete.

### Key Features

- **Automatic Cleanup**: Deletes the database automatically when the fixture is disposed
- **Manual Database Creation**: Ensures the database exists and the schema is applied via `EnsureCreatedAsync()`
- **DbContext Access**: Exposes the `DbContext` instance via the `LazyDbContext` property
- **Test Isolation**: When combined with [`TmpDatabaseNameFixture`](../asp-net-core/TmpDatabaseNameFixture.md), each test gets a unique temporary database

## Basic Usage

First, ensure your `DbContext` is registered in the application's DI container:

```csharp
// ASP.NET Core test application entry point (Program.cs)
public class Program
{
    public const string ConnectionStringName = "DefaultConnection";
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Register your DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString(ConnectionStringName))
        );
        var app = builder.Build();

        // Creates a default user for demonstration purposes
        app.MapPost("/user", async (ApplicationDbContext dbCtx) =>
        {
            dbCtx.Users.Add(new User()
            {
                Age = 100,
                Name = "test",
            });
            await dbCtx.SaveChangesAsync();
        });

        app.Run();
    }
}
```

Then use the fixture in your tests:

```csharp
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.EF;
using Microsoft.EntityFrameworkCore;

public class DatabaseTests
{
    // Request the fixture
    protected DatabaseLifecycleFixture<Program, ApplicationDbContext> EnsureDbFx => 
        FixtureSet.EnsureDbFx;

    // Property for convenient access
    protected ApplicationDbContext AppDbCtx => EnsureDbFx.LazyDbContext;

    [Fact]
    public async Task Create_User__should_persist_to_database()
    {
        // Ensure the database is created and migrations are applied
        await EnsureDbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        // Perform database operations
        // Send a POST request to the /user endpoint to create a new user
        // The API creates a default user when the request body is null
        var resp = await Client.PostAsync("/user", null, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Verify the data was persisted
        var users = await AppDbCtx.Users.ToListAsync(TestContext.Current.CancellationToken);
        users.Count.Should().Be(1);
        users[0].Name.Should().Be("Test");
    }
    // The database is automatically deleted after the test completes
}
```

## Key Members

| Property | Type | Description |
|----------|------|-------------|
| `LazyDbContext` | `TContext` | Gets the `DbContext` instance resolved from the service provider. Starts the application under test if it is not already running. |

| Method | Returns | Description |
|--------|---------|-------------|
| `EnsureCreatedAsync(CancellationToken)` | `Task` | Ensures that the database for the context exists and is created. Applies all pending migrations. Starts the application under test if it is not already running. |

> [!NOTE]
> The database is deleted in `DisposeAsync()` only if the application was started during the test.

## Advanced Usage: Combining with TmpDatabaseNameFixture

For complete test isolation, combine `DatabaseLifecycleFixture` with [`TmpDatabaseNameFixture`](../asp-net-core/TmpDatabaseNameFixture.md). This creates a unique temporary database for each test scope, ensuring tests don't interfere with each other.

### Example: Full Integration Setup

```csharp
using FEFF.TestFixtures.AspNetCore;
using FEFF.TestFixtures.AspNetCore.EF;

// Define options for TmpDatabaseNameFixture
[Fixture]
public class TestOptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => 
        [Program.ConnectionStringName];
}

// Both fixtures are instantiated together
[Fixture]
public record FixtureSet(
    DatabaseLifecycleFixture<Program, ApplicationDbContext> EnsureDbFx,
    TmpDatabaseNameFixture<Program, TestOptionsFixture> TmpDbNameFx
);

public class IsolatedDatabaseTests
{
    protected FixtureSet FixtureSet { get; } = 
        TestContext.Current.GetFeffFixture<FixtureSet>();

    protected DatabaseLifecycleFixture<Program, ApplicationDbContext> EnsureDbFx => 
        FixtureSet.EnsureDbFx;
    
    protected ApplicationDbContext AppDbCtx => 
        FixtureSet.EnsureDbFx.LazyDbContext;

    // Each test gets its own unique database
    [Fact]
    public async Task Create_User__should_persist_to_database()
    {
        // TmpDatabaseNameFixture automatically redirects the connection string
        // to a unique temporary database (e.g., "TestDb_a1b2c3d4")
        // This happens during AppManagerFixture setup, before the application is started and the database is created

        // Ensure the database is created and migrations are applied
        // This call starts the application
        await EnsureDbFx.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        // ... the test remains the same
    }
    // The database is automatically deleted after the test completes
}
```

### How It Works

1. **TmpDatabaseNameFixture** intercepts the connection string during application configuration and redirects it to a uniquely named temporary database (e.g., `TestDb_<guid>`)
2. **Test** starts the application by calling `EnsureDbFx.EnsureCreatedAsync()`
3. **DatabaseLifecycleFixture** creates the database and applies migrations to this unique database
4. Tests run in complete isolation with no cross-test contamination
5. The temporary database is automatically deleted after the test completes

### Benefits

- **Complete Isolation**: Each test scope gets its own database with no shared state
- **No Cleanup Required**: Temporary databases are automatically deleted on test completion
- **Parallel Execution**: Tests can run in parallel without database conflicts or race conditions
- **Clean Execution**: Tests start with a fresh database each time
- **Realistic Testing**: Tests run against a real database, not in-memory mocks

## See Also

| Link | Description |
|------|-------------|
| [API: DatabaseLifecycleFixture](xref:FEFF.TestFixtures.AspNetCore.EF.DatabaseLifecycleFixture`2) | API reference |
| [DatabaseLifecycleFixture Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore.EF/DatabaseLifecycleFixtureTests.cs) | Unit tests for `DatabaseLifecycleFixture` |
| [TmpDatabaseNameFixture](../asp-net-core/TmpDatabaseNameFixture.md) | Create unique temporary databases for test isolation |
| [API Integration Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Complete integration test examples |
