# FEFF.TestFixtures.AspNetCore

ASP.NET Core fixtures for the **FEFF.TestFixtures** solution.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This package provides fixtures for testing **ASP.NET Core** applications using `WebApplicationFactory<T>`. It simplifies application lifecycle management, HTTP client creation, and faking of common services.

### Quick Start

1. Add the package to your test project:

   ```bash
   dotnet add package FEFF.TestFixtures.AspNetCore
   ```

2. Ensure you have a test framework integration package installed (e.g., `FEFF.TestFixtures.XunitV3`).

3. Resolve fixtures in your tests:

   ```csharp
   var client = TestContext.Current.GetFeffFixture<AppClientFixture<Program>>();
   ```

### Fixtures Included

| Fixture | Description |
|---------|-------------|
| `AppManagerFixture<TEntryPoint>` | Starts/stops the application via `WebApplicationFactory`; allows configuration and service collection modification before startup |
| `AppServicesFixture<TEntryPoint>` | Resolves services from the application's `IServiceProvider` with scoped lifetime support |
| `AppClientFixture<TEntryPoint>` | Creates an `HttpClient` connected to the test application |
| `AuthorizedAppClientFixture<TEntryPoint>` | Creates an authenticated `HttpClient` with a Bearer token |
| `FakeTimeFixture<TEntryPoint>` | Replaces `TimeProvider` with a controllable `FakeTimeProvider` |
| `FakeLoggerFixture<TEntryPoint>` | Replaces `ILoggerProvider` with a `FakeLoggerProvider` for assertion on log output |
| `FakeRandomFixture<TEntryPoint>` | Replaces `Random.Shared` with a deterministic `FakeRandom` |
| `TmpDatabaseNameFixture<TEntryPoint>` | Appends a unique identifier to connection string database names for test isolation |

### Preview Features

Preview fixtures (subject to change) are shipped under the `FEFF.TestFixtures.AspNetCore.Preview` namespace.

## Examples

- [Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | Core fixtures library |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | Xunit v3 integration |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | TUnit integration |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | **current package** |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | EF Core database lifecycle fixture |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | SignalR testing fixture |
