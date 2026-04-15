# Troubleshooting & FAQ

This article addresses common questions and issues when working with FEFF.TestFixtures.

## Which package should I use?

Choose the package based on your test framework and requirements:

+ **xUnit v3 projects**: Install `FEFF.TestFixtures.XunitV3`
+ **TUnit projects**: Install `FEFF.TestFixtures.TUnit`
+ **ASP.NET Core integration tests**: Also install `FEFF.TestFixtures.AspNetCore`

### Package Reference Summary

| Scenario | Package |
|----------|---------|
| xUnit v3 tests | `FEFF.TestFixtures.XunitV3` |
| TUnit tests | `FEFF.TestFixtures.TUnit` |
| ASP.NET Core tests | `FEFF.TestFixtures.AspNetCore` |
| ASP.NET Core + EF Core tests | `FEFF.TestFixtures.AspNetCore.EF` |
| ASP.NET Core + SignalR tests | `FEFF.TestFixtures.AspNetCore.SignalR` |

## Why can't I resolve a fixture?

If you get a compilation or runtime error when trying to resolve a fixture, ensure you've added the assembly-level attribute (xUnit v3 only):

```csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

This attribute is required for xUnit v3 to initialize the extension and register fixtures with the test framework.

> [!NOTE]
> TUnit users do not need this attribute. The `FEFF.TestFixtures.TUnit` package handles registration automatically.

## Can fixtures share state across tests?

Yes. Class-, collection-, and assembly-scoped fixtures are singleton within their scope. All tests within that scope receive the same instance.

### Scope Types (for xUnit v3 integration)

| Scope | Behavior |
|-------|----------|
| `test-case` (default) | New instance for each test method |
| `class` | One instance per test class |
| `collection` | One instance per test collection |
| `assembly` | One instance for the entire test assembly |

### Example: Class-scoped Fixture

```csharp
public class MyTests
{
    // Class-scoped fixture - shared across all tests in this class
    protected MyFixture Shared { get; } = 
        TestContext.Current.GetFeffFixture<MyFixture>(FixtureScopeType.Class);

    [Fact]
    public void Test1() { /* uses Shared */ }

    [Fact]
    public void Test2() { /* uses same Shared instance */ }
}
```

## How do I clean up resources in a fixture?

Implement `IDisposable` or `IAsyncDisposable` on your fixture class. The engine will call `Dispose()` or `DisposeAsync()` at the end of the fixture's scope.

### Synchronous Disposal

```csharp
[Fixture]
public class TmpDirectoryFixture : IDisposable
{
    public string Path { get; } = Directory.CreateTempSubdirectory().FullName;

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (DirectoryNotFoundException)
        {
            // Ignore if already deleted
        }
    }
}
```

### Asynchronous Disposal

```csharp
[Fixture]
public class DatabaseFixture : IAsyncDisposable
{
    private readonly IDbConnection _connection;

    public async ValueTask DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }
}
```

> [!TIP]
> Use `IAsyncDisposable` for fixtures that manage async resources like database connections, network sockets, or file handles.

## Common Issues

### Fixture Dependency Cycles

Fixtures cannot have cyclic dependencies. The following will cause an error:

```csharp
[Fixture]
public class FixtureA
{
    public FixtureA(FixtureB b) { } // Error: Circular dependency
}

[Fixture]
public class FixtureB
{
    public FixtureB(FixtureA a) { }
}
```

**Solution**: Refactor to break the cycle. Consider introducing a third fixture that both depend on.

### Fixture Not Found at Runtime

If you get an exception about a missing fixture:

1. Verify the fixture type has the `[Fixture]` attribute
2. Ensure the fixture is in an assembly that's referenced by your test project
3. Check that all fixture dependencies are also properly registered

### Multiple Fixture Instances in Same Scope

If you notice unexpected multiple instances of a fixture:

```csharp
var fx1 = TestContext.Current.GetFeffFixture<MyFixture>();
var fx2 = TestContext.Current.GetFeffFixture<MyFixture>();

// fx1 and fx2 should be the same instance in the same scope
```

Ensure you're using the same scope type and not accidentally creating new scopes.

## Where to Report Issues or Ask Questions

+ **Bug Reports**: [GitHub Issues](https://github.com/metacoder-feff/FEFF.TestFixtures/issues)
+ **Questions & Discussions**: [GitHub Discussions](https://github.com/metacoder-feff/FEFF.TestFixtures/discussions)

Before opening a new issue, please:
1. Search existing issues to avoid duplicates
2. Include your .NET version, test framework version, and FEFF.TestFixtures version
3. Provide a minimal reproducible example when possible
