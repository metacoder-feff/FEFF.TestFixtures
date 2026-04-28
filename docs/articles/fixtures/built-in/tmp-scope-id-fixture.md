# TmpScopeIdFixture

> **Assembly**: `FEFF.TestFixtures.dll`  
> **Namespace**: `FEFF.TestFixtures`  
> **Source**: [`TmpScopeIdFixture.cs`](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures/Fixtures/TmpScopeIdFixture.cs)

## Overview

`TmpScopeIdFixture` provides a unique identifier string for each fixture scope. Every call to the fixture within the same scope returns the same instance, while different scopes receive different unique identifiers. This is useful for creating isolated test data, unique naming prefixes, or scope-specific identifiers. This Fixture is intended to be used as a dependency for other fixtures (e.g. `TmpDatabaseNameFixture`).

## Basic Usage

Retrieve the fixture from the test context:

```csharp
using FEFF.TestFixtures;

public class UniqueIdTests
{
    protected TmpScopeIdFixture ScopeId { get; } = 
        TestContext.Current.GetFeffFixture<TmpScopeIdFixture>();

    [Fact]
    public void ScopeId__should_be_unique_per_scope()
    {
        // Arrange
        var uniqueId = ScopeId.Value;

        // Act & Assert
        uniqueId.Should().NotBeNullOrEmpty();
        uniqueId.Should().BeOfType<string>();
    }
}
```

### Key Members

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `string` | A unique (GUID) string for this fixture scope |

## Use Cases

### 1. Unique Test Data Prefixes

Generate unique prefixes for test data to avoid collisions:

```csharp
[Fact]
public void Redis__should_store_data_with_unique_prefix()
{
    var prefix = $"test-{ScopeId.Value}";
    var key = $"{prefix}:mykey";
    
    // Use key for Redis operations
    // Each test gets a unique prefix, preventing conflicts
}
```

### 2. Database Schema Isolation

Create unique schema names for database tests:

```csharp
[Fact]
public void Database__should_use_unique_schema()
{
    var schemaName = $"test_{ScopeId.Value.Replace("-", "_")}";
    
    // Create schema-specific tables
    // Ensures test isolation without dropping databases
}
```

## See Also

| Resource | Description |
|----------|-------------|
| [API: TmpScopeIdFixture](xref:FEFF.TestFixtures.TmpScopeIdFixture) | API reference |
| [Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures/TmpScopeIdFixtureTests.cs) | Core functionality and scope isolation tests |
| [TmpDatabaseNameFixture](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures.AspNetCore/Fixtures/TmpDatabaseNameFixture.cs) | Example of using `TmpScopeIdFixture` in ASP.NET Core |
