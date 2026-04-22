# Fixture Parameterization

Fixture parameterization allows you to create fixture instances with concrete argument values by defining options types that supply configuration data. This pattern is essential when you need the same fixture to behave differently across tests based on custom parameters.

## Overview

While standard fixtures are created with default behavior, parameterized fixtures accept configuration through a dedicated options type. This options type is itself a fixture, enabling dependency injection and test isolation.

The pattern consists of three components:

1. **Options Interface** - Defines the contract for configuration data
2. **Options Fixture** - Implements the interface with concrete values
3. **Parameterized Fixture** - Consumes the options fixture via constructor injection of generic type parameter

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
            $"test-{Guid.New()}", 
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
- Return hardcoded values
- Read from environment variables
- Depend on other fixtures for dynamic configuration

### Step 4: Use the Parameterized Fixture

Create the fuxture instance:

```csharp
var fixtureInstance = TestContext.Current.GetFeffFixture<TmpDatabaseNameFixture<OptionsFixture>>();
//...
```

The generic type parameter `OptionsFixture` tells `TmpDatabaseNameFixture` which options to use.

> [!TIP]
> Single `OptionsFixture` class can implement multiple different interfaces to parameterize multiple fixtures used in a test suite.

## Benefits of Fixture Parameterization

- **Type Safety** - Options are compile-time checked via generics
- **Composability** - Options fixtures can depend on other fixtures
- **Reusability** - Same parameterized fixture works with different options
- **Clarity** - Options type documents required configuration

## See Also

| Resource | Description |
|----------|-------------|
| [TmpDatabaseNameFixture.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/TmpDatabaseNameFixture.cs) | Parameterized fixture implementation |
| [ITmpDatabaseNameFixtureOptions.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/TmpDatabaseNameFixture.cs) | Options interface definition |
| [ApiTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs) | Example usage in API tests |
| [TmpDatabaseNameFixtureTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures.AspNetCore/TmpDatabaseNameFixtureTests.cs) | Unit tests for parameterization |
| [Configuring Fixtures](configuring-fixtures.md) | Alternative configuration via environment variables |
