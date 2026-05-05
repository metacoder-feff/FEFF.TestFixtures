# Generic Parameterization Pattern

## Overview

Fixture parameterization lets you pass values to a fixture that are required but not known when the fixture is created. Instead of hardcoding configuration inside the fixture, you supply it from the outside.

This is useful when:

- Different tests need the same fixture to behave differently
- Configuration values (connection strings, endpoints, flags) are decided at test time, not when the fixture is written
- The same fixture is reused across test suites with different settings

The pattern uses three pieces:

1. **Options Interface** — defines what values the fixture needs
2. **Options Fixture** — provides the actual values
3. **Parameterized Fixture** — receives the values through its constructor

## Example: TmpDatabaseNameFixture

The `TmpDatabaseNameFixture` demonstrates this pattern by allowing tests to specify which connection strings should be redirected to temporary databases.

### Step 1: Define the Options Interface

```csharp
public interface ITmpDatabaseNameFixtureOptions
{
    IReadOnlyCollection<string> ConnectionStringNames { get; }
}
```

This interface specifies what configuration data the parameterized fixture needs.

### Step 2: Create a Parameterized Fixture

The parameterized fixture is a generic class that accepts the options type as a type parameter:

```csharp
[Fixture]
public class TmpDatabaseNameFixture<TOptionsFixture>
where TOptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public TmpDatabaseNameFixture(
        AppManagerFixture app,
        TOptionsFixture opts)
    {
        app.ConfigurationBuilder.UseDatabaseNamePostfix(
            $"test-{Guid.NewGuid()}", 
            opts.ConnectionStringNames);
    }
}
```

Key characteristics:
- **Generic type parameter** `TOptionsFixture` - The options fixture type
- **Constraint** `where TOptionsFixture : ITmpDatabaseNameFixtureOptions` - Ensures the options type implements the required interface
- **Constructor injection** - Receives the options fixture via DI alongside other dependencies

### Step 3: Create an Options Fixture

Create a fixture that implements the options interface:

```csharp
[Fixture]
public class OptionsFixture : ITmpDatabaseNameFixtureOptions
{
    public IReadOnlyCollection<string> ConnectionStringNames => 
        ["DefaultConnection"];
}
```

The options fixture can:

- Return hardcoded or calculated values
- Depend on other fixtures for dynamic configuration

### Step 4: Use the Parameterized Fixture

Create the fixture instance:

```csharp
var fixtureInstance = TestContext.Current.GetFeffFixture<TmpDatabaseNameFixture<OptionsFixture>>();
//...
```

The generic type parameter `OptionsFixture` tells `TmpDatabaseNameFixture` which options to use.

> [!TIP]
> A single `OptionsFixture` class can implement multiple different interfaces to parameterize multiple fixtures used in a test suite.

## Benefits of Fixture Parameterization

- **Type Safety** - Options are compile-time checked via generics
- **Composability** - Options fixtures can depend on other fixtures
- **Reusability** - Same parameterized fixture works with different options
- **Clarity** - Options type documents required configuration

## See Also

| Resource | Description |
|----------|-------------|
| [TmpDatabaseNameFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/TmpDatabaseNameFixture.cs) | Parameterized fixture and options interface |
| [ApiTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Example usage in API tests |
| [TmpDatabaseNameFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/TmpDatabaseNameFixtureTests.cs) | Unit tests for parameterization |
| [Options Pattern](options-pattern.md) | Alternative configuration via environment variables |
| [Selecting Configuration Method](selecting-configuration-method.md) | Comparison with Options Pattern |
