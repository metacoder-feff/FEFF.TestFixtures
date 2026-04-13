# FEFF.TestFixtures.XunitV3

Xunit v3 integration for the **FEFF.TestFixtures** solution.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This package integrates the fixture engine with **Xunit v3**, enabling fixture resolution through the xUnit test context.

### Quick Start

1. Add the package to your test project:

   ```bash
   dotnet add package FEFF.TestFixtures.XunitV3
   ```

2. Enable the extension at the assembly level:

   ```csharp
   [assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
   ```

3. Resolve fixtures in your tests:

   ```csharp
   var tmpDir = TestContext.Current.GetFeffFixture<TmpDirectoryFixture>();
   var scoped = TestContext.Current.GetFeffFixture<MyFixture>(FixtureScopeType.Class);
   ```

### Supported Scopes

| Scope | Description |
|-------|-------------|
| `TestCase` | Created and destroyed per test case |
| `Class` | Shared across all tests in a test class |
| `Collection` | Shared across all tests in a test collection |
| `Assembly` | Shared across all tests in the assembly |

## Examples

- [Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs)
- [AspNetCore example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | Core fixtures library |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | **current package** |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | TUnit integration |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | ASP.NET Core fixtures |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | EF Core database lifecycle fixture |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | SignalR testing fixture |
