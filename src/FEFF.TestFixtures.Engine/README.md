# FEFF.TestFixtures.Engine

Core engine for the **FEFF.TestFixtures** solution.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This package is the **engine** that powers the entire fixture system. It is test-framework-agnostic and can be used standalone or through a test-framework integration package.

Key components:

| Type | Description |
|------|-------------|
| `FixtureManager` | Creates, memoizes, and disposes fixtures within scopes |
| `FixtureManagerBuilder` | Configures and builds a `FixtureManager` |
| `IFixtureScope` | Interface for resolving fixtures within a scoped lifetime |

The engine discovers fixture classes via reflection (types marked with `[Fixture]` or implementing `IFixtureRegistrar`) and manages their lifecycle, dependency injection, and disposal.

**You typically don't reference this package directly** — use a test-framework integration package instead (e.g., `FEFF.TestFixtures.XunitV3` or `FEFF.TestFixtures.TUnit`). The integration packages will automatically include this engine as a dependency.

## Examples

See integration examples in:

- [Xunit v3 example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs)
- [TUnit example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.TUnit/ExampleTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | Core fixtures library |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | Xunit v3 integration |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | TUnit integration |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | ASP.NET Core fixtures |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | EF Core database lifecycle fixture |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | SignalR testing fixture |
