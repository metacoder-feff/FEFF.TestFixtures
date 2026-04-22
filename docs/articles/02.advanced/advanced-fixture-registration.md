# Advanced Fixture Registration

While the `[Fixture]` attribute provides a simple way to discover and register fixtures, FEFF.TestFixtures offers more advanced registration patterns for fixtures that require configuration or custom dependency injection setup.

## Two Major Registration Approaches

There are two primary ways to register fixtures with the FEFF.TestFixtures engine:

### 1. Attribute-Based Discovery (Simple)

The simplest approach uses the `[Fixture]` attribute on your fixture class. The engine automatically discovers and registers these fixtures:

```csharp
[Fixture]
public class MySimpleFixture
{
    public MySimpleFixture()
    {
        // Constructor-based initialization
    }
}
```

This approach works well for fixtures that don't require external configuration.

### 2. Advanced Registration with IFixtureRegistrar

For fixtures that need configuration options or custom DI setup, implement the `IFixtureRegistrar` interface on any public class and provide a static `RegisterFixture(IServiceCollection services)` method:

```csharp
public class MyFixtureRegistrar : IFixtureRegistrar
{
    public static void RegisterFixture(IServiceCollection services)
    {
        // Custom registration logic
        services.Configure<MyOptions>(options => 
        {
            options.SomeSetting = "value";
        });
    }
}
```

This approach gives you full control over how the fixture and its dependencies are registered in the DI container.

> [!TIP]
> The `DiscoveryService` searches for any public class that implements `IFixtureRegistrar`.

## Configuring Fixtures

Fixtures that implement `IFixtureRegistrar` can expose configuration options through the `Microsoft.Extensions.Options` pattern. See the dedicated article for a complete walkthrough:

👉 [Configuring Fixtures](configuring-fixtures.md) - Learn how to add configuration support to fixtures like `TmpDirectoryFixture`.

## When to Use Advanced Registration

Use advanced registration when you need to:

- **Provide configuration options** - Allow users to customize fixture behavior
- **Register additional services** - Register supporting services alongside the fixture
- **Custom lifetime management** - Control how and when the fixture is disposed
- **Integration with external systems** - Configure external dependencies before fixture creation

## See Also

| Resource | Description |
|----------|-------------|
| [IFixtureRegistrar.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Abstractions/Registration/IFixtureRegistrar.cs) | Interface definition for custom registration |
| [FixtureAttribute.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Abstractions/Registration/FixtureAttribute.cs) | Attribute for fixture discovery |
| [AssemblyDiscoveryService.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Engine/Engine/AssemblyDiscoveryService.cs) | Service for discovering fixtures via reflection |
| [ReflectiveFixtureCollector.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.Engine/Engine/ReflectiveFixtureCollector.cs) | Collector for fixture types from assemblies |
