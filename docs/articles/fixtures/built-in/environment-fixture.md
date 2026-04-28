# EnvironmentFixture

> **Assembly**: `FEFF.TestFixtures.dll`  
> **Namespace**: `FEFF.TestFixtures`  
> **Source**: [`EnvironmentFixture.cs`](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures/Fixtures/EnvironmentFixture.cs)


## Overview

`EnvironmentFixture` captures a snapshot of all environment variables when instantiated and reverts any changes when disposed. This ensures tests don't interfere with each other through environment variable modifications.

> [!IMPORTANT]
> Environment variable manipulation is process-wide. Tests using this fixture must run sequentially, not in parallel. Attempt to run in parallel would throw `InvalidOperationException`

> [!TIP]
> For xUnit, use the `[Collection]` attribute to ensure tests run sequentially.

## Basic Usage

Retrieve the fixture from the test context:

```csharp
using FEFF.TestFixtures;

public class SystemUnderTest
{
    public static string? GetValue() => Environment.GetEnvironmentVariable("TEST_VAR");
}

public class EnvironmentTests
{
    protected EnvironmentFixture Env { get; } = 
        TestContext.Current.GetFeffFixture<EnvironmentFixture>();

    [Fact]
    public void Environment_variable__should_be_restored_after_test()
    {
        // Modify environment variable
        const string key = "TEST_VAR";
        Env.SetEnvironmentVariable(key, "modified");

        // Use environment variable in a tested code
        var value = SystemUnderTest.GetValue();
        value.Should().Be("modified");

        // Cleanup - fixture restores automatically
        // Env.Dispose();

        // Assert - after dispose
        // string? initial = null;
        // Environment.GetEnvironmentVariable(key).Should().Be(initial);
    }
}
```

> [!TIP]
> 'EnvironmentFixture.SetEnvironmentVariable' is the same as 'Environment.SetEnvironmentVariable' and is used for better readability and not to forget to instantiate `EnvironmentFixture`


### Key Members

| Property | Type | Description |
|----------|------|-------------|
| `InitialSnapshot` | `FrozenDictionary<string, string>` | Immutable snapshot of environment variables at fixture creation |


| Method | Description |
|--------|-------------|
| `SetEnvironmentVariable(string, string?)` | Convenience wrapper around `Environment.SetEnvironmentVariable()` |

## Sequential Execution

For xUnit, use the `[Collection]` attribute to ensure tests run sequentially:

```csharp
public static class Consts
{
    public const string EnvironmentTests = "EnvironmentTests";
}

[Collection(Consts.EnvironmentTests)]
public class TestClass1
{
    protected EnvironmentFixture Env { get; } = 
        TestContext.Current.GetFeffFixture<EnvironmentFixture>();

    [Fact]
    public void Test1() { /* ... */ }
}

[Collection(Consts.EnvironmentTests)]
public class TestClass2
{
    protected EnvironmentFixture Env { get; } = 
        TestContext.Current.GetFeffFixture<EnvironmentFixture>();

    [Fact]
    public void Test2() { /* ... */ }
}
```

Tests within the same collection run sequentially, preventing parallel environment variable conflicts.

## Best Practices

1. **Always use `[Collection]` attribute**: Ensure sequential execution to prevent race conditions between tests.

2. Use a `const` field for `CollectionName` to avoid misspelling.

2. **Minimize environment modifications**: Only set environment variables that are absolutely necessary for your tests.

## Troubleshooting

### Tests Fail with "Cannot be used in Parallel"

```
'EnvironmentFixture' mutates process environment and cannot be used in parallel. 
Ensure tests using this fixture run sequentially. For xUnit, consider using the 
[Collection] attribute on all test classes that will be part of a collection. 
Tests within the same collection run sequentially.
```

**Cause**: Multiple test classes using `EnvironmentFixture` without proper collection definition.

**Solution**: Define a collection and apply it to all affected test classes:

## See Also

| Resource | Description |
|----------|-------------|
| [API: EnvironmentFixture](xref:FEFF.TestFixtures.EnvironmentFixture) | API reference |
| [Unit Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures/EnvironmentFixtureTest.cs) | Core functionality test coverage |
