# FEFF.TestFixtures.AspNetCore.EF

Entity Framework Core fixtures for the **FEFF.TestFixtures** solution.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This package provides a fixture for managing **EF Core database lifecycle** in integration tests. It simplifies the creation of the database and automatically removes it when the fixture scope ends.

### Quick Start

1. Add the package to your test project:

   ```bash
   dotnet add package FEFF.TestFixtures.AspNetCore.EF
   ```

2. Resolve the fixture in your tests:

   ```csharp
   var db = TestContext.Current.GetFeffFixture<DatabaseLifecycleFixture<Program, AppDbContext>>();
   ```

### Fixtures Included

| Fixture | Description |
|---------|-------------|
| `DatabaseLifecycleFixture<TEntryPoint, TContext>` | Calls `DbContext.Database.EnsureCreated()` on setup and `DbContext.Database.EnsureDeleted()` on disposal, scoped to the fixture lifetime (e.g., per test case) |

Use this fixture alongside `AppManagerFixture` to ensure a clean database for each test or test scope.

## Examples

- [AspNetCore example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | Core fixtures library |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | Xunit v3 integration |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | TUnit integration |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | ASP.NET Core fixtures |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | **current package** |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | SignalR testing fixture |
