# Fixture Factory Internals

The `FixtureManager` is the core component responsible for creating, caching, and managing the lifecycle of fixture scopes. It acts as a factory and coordinator for all fixture instances within the test execution pipeline.

## Core Components

### FixtureManager

The central manager class that orchestrates fixture lifecycle:

```csharp
public sealed class FixtureManager : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly Dictionary<string, FixtureScope> _scopes;
    private readonly object _lock;
}
```

**Key Responsibilities:**
- Creates new `FixtureScope` instances on demand
- Caches scopes in a thread-safe dictionary
- Provides `GetScope(string id)` for retrieving or creating scopes
- Manages global disposal through `DisposeAsync()`
- Supports individual scope removal via `RemoveScopeAsync(string scopeId)`

### FixtureScope

Represents an isolated fixture container with its own DI scope:

```csharp
internal sealed class FixtureScope : IAsyncDisposable, IFixtureScope
{
    private readonly AsyncServiceScope _serviceScope;

    public T GetFixture<T>() where T : notnull;
    public ValueTask DisposeAsync();
}
```

**Key Responsibilities:**
- Wraps `AsyncServiceScope` from Microsoft.Extensions.DependencyInjection
- Resolves fixtures through dependency injection
- Handles async disposal of scoped services

## Usage Patterns

### Creating a Scope

Scopes are created lazily when first requested:

```csharp
var fixtureManager = new FixtureManagerBuilder()
    .Build();

// First call creates the scope
IFixtureScope scope1 = fixtureManager.GetScope("test-case-1");

// Second call returns cached scope
IFixtureScope scope2 = fixtureManager.GetScope("test-case-1");

// scope1 and scope2 reference the same instance
```

### Individual Scope Removal

Removes and disposes a specific scope:

```csharp
await fixtureManager.RemoveScopeAsync("test-case-1");
```

The scope is removed from the cache and disposed asynchronously. If the scope doesn't exist, the operation completes silently.


### Global Disposal

Disposes all scopes and the service provider:

```csharp
await fixtureManager.DisposeAsync();
```

This iterates through all cached scopes and disposes them in order, followed by the root `ServiceProvider`.

## Lifecycle Management

### Fixture Registration and Discovery

Before scopes can be created, fixtures must be discovered and registered with the DI container.

**Discovery Process:**

1. Framework scans assemblies for types marked with `[Fixture]` or types that implement `IFixtureRegistrar`
2. Registers fixtures in the `ServiceCollection`
3. Builds the root `ServiceProvider` for scope creation
4. `ServiceProvider` is used by `FixtureManager` to resolve fixures

### Scope Creation Flow

1. User calls `GetScope(id)`
2. Manager checks if scope exists (thread-safe)
3. If not exists, creates new `FixtureScope` with DI container
4. Caches scope by identifier
5. Returns scope instance

### Scope Disposal Flow

1. User calls `DisposeAsync()` or `RemoveScopeAsync(id)`
2. Manager collects all disposables
3. Disposes fixtures in reverse registration order
4. Disposes underlying `ServiceProvider`
5. Marks manager as disposed (prevents further scope creation)

## Thread Safety

`FixtureManager` is designed for concurrent access:

- Uses `Lock` (NET9.0+) or `object` (legacy) for synchronization
- Double-check pattern prevents unnecessary lock acquisition
- Disposal flag prevents operations on disposed manager
- Each `FixtureScope` is isolated and thread-confined

## Dependency Injection Integration

The fixture factory leverages `Microsoft.Extensions.DependencyInjection`:

- Root `ServiceProvider` is created from configured services
- Each scope creates child `AsyncServiceScope`
- Fixtures are created within one of scopes
- Fixtures are resolved via `AsyncServiceScope.ServiceProvider.GetRequiredService<T>()`
- Fixtures are disposed on scope cleanup

## Error Handling

- **Null/Empty IDs**: Throws `ArgumentException`
- **Disposed Manager**: Throws `ObjectDisposedException`
- **Missing Fixtures**: Throws from DI container (`InvalidOperationException`)
- **Concurrent Access**: Safely handled via locking

## See Also

### Sources

- [FixtureManager.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Engine/Engine/FixtureManager.cs)
- [FixtureScope.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Engine/Engine/FixtureScope.cs)
- [FixtureManagerBuilder.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Engine/Engine/FixtureManagerBuilder.cs)
- [AssemblyDiscoveryService.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Engine/Engine/AssemblyDiscoveryService.cs)

### Tests

- [FixtureManagerTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/EngineTests/FixtureManagerTests.cs)
- [FixtureScopeTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/EngineTests/FixtureScopeTests.cs)
- [ConcurrentAccessTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/EngineTests/ConcurrentAccessTests.cs)
