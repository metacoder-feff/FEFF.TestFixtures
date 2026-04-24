# List of Fixtures

FEFF.TestFixtures provides a set of ready-to-use fixtures for common testing scenarios. These fixtures are managed by the framework and automatically cleaned up after use.

## Built-in Fixtures 
`FEFF.TestFixtures.dll`

| Fixture | Description |
|---------|-------------|
| `TmpDirectoryFixture` | Creates a unique temp directory, auto-deletes after test |
| `EnvironmentFixture` | Snapshots process environment, restores after test |
| `TmpScopeIdFixture` | Generates unique ID per scope (for test isolation) |

## ASP.NET Core Fixtures
`FEFF.TestFixtures.AspNetCore.dll`

| Fixture | Description |
|---------|-------------|
| `AppManagerFixture` | Manages test host lifecycle for web apps |
| `AppClientFixture` | Provides `HttpClient` for API testing |
| `AppServicesFixture` | Access to application's service provider |
| `FakeTimeFixture` | Replaces `TimeProvider` with controllable fake |
| `FakeRandomFixture` | Replaces `Random` with deterministic fake |
| `FakeLoggerFixture` | Captures logs for assertions |
| `TmpDatabaseNameFixture` | Creates unique database name per test |

## ASP.NET Core + Entity Framework Core Extension
`FEFF.TestFixtures.AspNetCore.EF.dll`

| Fixture | Description |
|---------|-------------|
| `DatabaseLifecycleFixture` | Creates/deletes database automatically |

## Preview Fixtures
Fixtures that are implemented but not fully tested.

### ASP.NET Core Fixtures
`FEFF.TestFixtures.AspNetCore.dll`

| Fixture | Description |
|---------|-------------|
| `AuthorizedAppClientFixture` | Provides authenticated `HttpClient` with Bearer token for testing protected APIs |


### ASP.NET Core + SignalR Extension
`FEFF.TestFixtures.AspNetCore.SignalR.dll`

| Fixture | Description |
|---------|-------------|
| `SignalrClientFixture` | Creates and manages a SignalR test client connected to the application's hub; provides awaitable events for server-sent messages |

## Planned
Fixtures that are being developed.

| Fixture | Description |
|---------|-------------|
| `TmpRedisPrefixFixture` | Adds a unique Redis key prefix for test isolation |
| `TmpS3PrefixFixture` | Adds a unique S3 path prefix for test isolation |
| `MockHttpFixture` | Configures Fake outbound HTTP connection to stub integrations with third-party APIs (integrates [richardszalay/mockhttp](https://github.com/richardszalay/mockhttp) into testing pipeline) |
