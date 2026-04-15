# FEFF.TestFixtures

Core fixture library for the **FEFF.TestFixtures** solution.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This is the **core fixture library** — it ships a set of general-purpose fixtures that are useful in any test project, regardless of what is being tested.

### Quick Start

1. Add a test framework integration package to your test project:

   ```bash
   dotnet add package FEFF.TestFixtures.XunitV3
   # or
   dotnet add package FEFF.TestFixtures.TUnit
   ```

2. Enable the extension (xUnit v3 only):

   ```csharp
   [assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
   ```

3. Package `FEFF.TestFixtures` will be linked automatically.

4. Resolve fixtures in your tests:

   ```csharp
   var tmpDir = TestContext.Current.GetFeffFixture<TmpDirectoryFixture>();
   var env = TestContext.Current.GetFeffFixture<EnvironmentFixture>();
   ```

### Fixtures Included

| Fixture | Description |
|---------|-------------|
| `EnvironmentFixture` | Snapshots process environment variables on creation and restores them on disposal |
| `TmpDirectoryFixture` | Creates a unique temp directory per scope and deletes it on disposal; supports `Options.SkipDelete` to preserve the directory |
| `TmpScopeIdFixture` | Generates a unique GUID string per scope, useful for naming databases, files, or other isolated resources |

## Examples

- [Xunit v3 example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs)
- [TUnit example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.TUnit/ExampleTests.cs)
- [AspNetCore example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | **current package** |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | Xunit v3 integration |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | TUnit integration |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | ASP.NET Core fixtures |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | EF Core database lifecycle fixture |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | SignalR testing fixture |
