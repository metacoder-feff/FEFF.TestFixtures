# Add/Get Fixture by Interface

By default, fixtures are registered and retrieved by their concrete implementation type. However, FEFF.TestFixtures supports registering fixtures by interface, enabling more flexible and testable designs.

## Registering Fixture by Interface

To register a fixture by interface, use the `RegisterWithType` parameter in the `[Fixture]` attribute:

```csharp
internal interface IMyFixture
{
    string Value { get; }
}

[Fixture(RegisterWithType = typeof(IMyFixture))]
internal class MyFixture : IMyFixture
{
    public string Value { get; } = "Hello";
}
```

With this configuration, the fixture can be retrieved using either the implementation type or the interface type.

## Retrieving Fixtures

When a fixture is registered with `RegisterWithType`, it becomes accessible through both types:

```csharp
// Retrieve by implementation type
MyFixture fixture1 = Helper.GetFixture<MyFixture>();

// Retrieve by interface type
IMyFixture fixture2 = Helper.GetFixture<IMyFixture>();

// Both references point to the same instance
Assert.Same(fixture1, fixture2);
```

This dual registration enables:

- **Loose coupling**: Tests and other fixtures can depend on interfaces rather than concrete implementations
- **Better abstraction**: Fixture implementation details are hidden behind interfaces

> [!TIP]
> The type specified in `RegisterWithType` must be an interface or base class that the fixture implementation actually implements. Attempting to register with an incompatible type will result in an `InvalidCastException` at **testing time**.

## Multiple Interface Registrations

A single fixture can be registered with multiple interfaces by combining the attribute:

```csharp
[Fixture(RegisterWithType = typeof(IFixtureA))]
[Fixture(RegisterWithType = typeof(IFixtureB))]
internal class MultiInterfaceFixture : IFixtureA, IFixtureB
{
    // Implementation
}
```

## See Also

| Link | Description |
|------|-------------|
| [FixtureInterfaceTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/EngineTests/FixtureInterfaceTests.cs) | Unit tests for fixture interface registration |
