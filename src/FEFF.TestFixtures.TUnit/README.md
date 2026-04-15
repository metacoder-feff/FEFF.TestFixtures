# FEFF.TestFixtures.TUnit

TUnit integration for the **FEFF.TestFixtures** solution.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This package integrates the fixture engine with **TUnit**, enabling fixture resolution through the TUnit test context.

### Quick Start

1. Add the package to your test project:

   ```bash
   dotnet add package FEFF.TestFixtures.TUnit
   ```

2. Resolve fixtures in your tests:

   ```csharp
   var tmpDir = TestContext.Current.GetFeffFixture<TmpDirectoryFixture>();
   var scoped = TestContext.Current.GetFeffFixture<MyFixture>(FixtureScopeType.Class);
   ```

### Supported Scopes

| Scope | Description |
|-------|-------------|
| `TestCase` | Created and destroyed per test case |
| `Class` | Shared across all tests in a test class |
| `Assembly` | Shared across all tests in the assembly |
| `Session` | Shared across the entire test session |

## Examples

- [Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.TUnit/ExampleTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | Core fixtures library |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | Xunit v3 integration |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | **current package** |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | ASP.NET Core fixtures |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | EF Core database lifecycle fixture |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | SignalR testing fixture |
