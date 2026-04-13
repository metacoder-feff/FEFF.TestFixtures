# FEFF.TestFixtures.AspNetCore.SignalR

SignalR testing fixtures for the **FEFF.TestFixtures** solution.

> **Note:** This package is in **alpha** — the API may change in future releases. Breaking changes will be documented in release notes.

## About

Part of the **FEFF.TestFixtures** ecosystem — a framework-agnostic library for creating reusable test fixtures with scoped lifetimes. It replaces setup/teardown methods and the disposable pattern on test classes with composable, dependency-injected fixtures that can be shared across test projects.
See the [main README](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/README.md) for full documentation.

## This Package

This package provides a fixture for testing **SignalR** hubs in ASP.NET Core integration tests. It creates a SignalR test client connected to the test application's hub and captures server-sent events via Channels for async verification.

### Quick Start

1. Add the package to your test project:

   ```bash
   dotnet add package FEFF.TestFixtures.AspNetCore.SignalR
   ```

2. Resolve the fixture in your tests:

   ```csharp
   var signalr = TestContext.Current.GetFeffFixture<SignalrClientFixture<Program>>();
   ```

### Fixtures Included

| Fixture | Description |
|---------|-------------|
| `SignalrClientFixture<TEntryPoint, TOptions>` | Creates and manages a SignalR client tied to the `AppManagerFixture` lifecycle; provides awaitable events for server messages |

Supporting types:

- `SignalrTestClient` — test client that captures server events
- `ServerEvent` — represents a captured SignalR server event
- `TestServerExtensions` — extension methods for creating SignalR clients from `TestServer`

## Examples

- [AspNetCore example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs)

## See Also

| Package | Description |
|---------|-------------|
| [FEFF.TestFixtures](https://www.nuget.org/packages/FEFF.TestFixtures) | Core fixtures library |
| [FEFF.TestFixtures.XunitV3](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3) | Xunit v3 integration |
| [FEFF.TestFixtures.TUnit](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit) | TUnit integration |
| [FEFF.TestFixtures.AspNetCore](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore) | ASP.NET Core fixtures |
| [FEFF.TestFixtures.AspNetCore.EF](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF) | EF Core database lifecycle fixture |
| [FEFF.TestFixtures.AspNetCore.SignalR](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR) | **current package** |
