# Fixture Dependencies

Fixtures can depend on other fixtures through constructor injection. This allows you to compose complex test setups from simpler, reusable components.

## Basic Concept

When a fixture has other fixtures as constructor parameters, the framework automatically resolves and injects them. All fixture dependencies share the same scope as the dependent fixture.

## Example: Simple Dependency

Let's create a fixture that depends on another fixture:

### Step 1: Create a Base Fixture

First, create a simple fixture that provides a base resource:

```csharp
using FEFF.TestFixtures;

[Fixture]
public class TmpDirectoryFixture
{
    public string Path { get; }

    public TmpDirectoryFixture()
    {
        Path = Directory.CreateTempSubdirectory().FullName;
    }
}
```

### Step 2: Create a Dependent Fixture

Now create a fixture that depends on `TmpDirectoryFixture`:

```csharp
using FEFF.TestFixtures;

[Fixture]
public class LogFileFixture
{
    public string LogFilePath { get; }

    public LogFileFixture(TmpDirectoryFixture tmpDir)
    {
        LogFilePath = Path.Combine(tmpDir.Path, "app.log");
        File.WriteAllText(LogFilePath, string.Empty);
    }
}
```

### Step 3: Use in Tests

```csharp
using FEFF.TestFixtures;

public class LogFileTests
{
    protected LogFileFixture LogFile { get; } = 
        TestContext.Current.GetFeffFixture<LogFileFixture>();

    [Fact]
    public void LogFile__should_be_created()
    {
        // Arrange
        File.Exists(LogFile.LogFilePath).Should().BeFalse();

        // Act
        File.WriteAllText(LogFile.LogFilePath, "test log entry");

        // Assert
        File.Exists(LogFile.LogFilePath)
            .Should().BeTrue();
        
        File.ReadAllText(LogFile.LogFilePath)
            .Should().Be("test log entry");
    }
}
```

## Multiple Dependencies

Fixtures can depend on multiple other fixtures:

```csharp
[Fixture]
public class DatabaseTestFixture
{
    public string ConnectionString { get; }

    public DatabaseTestFixture(
        TmpDirectoryFixture tmpDir,
        EnvironmentFixture env)
    {
        var dbPath = Path.Combine(tmpDir.Path, "test.db");
        ConnectionString = $"Data Source={dbPath}";
    }
}
```

## Record-Based Fixtures

For cleaner syntax, you can use C# records:

```csharp
[Fixture]
public record TestEnvironmentFixture(
    TmpDirectoryFixture TempDir,
    EnvironmentFixture Environment,
    TmpScopeIdFixture TstId
);

public class ComplexTests
{
    protected TestEnvironmentFixture Env { get; } = 
        TestContext.Current.GetFeffFixture<TestEnvironmentFixture>();

    [Fact]
    public void Test__example()
    {
        // Access dependencies directly
        var tempPath = Env.TempDir.Path;
        var randomValue = Env.TstId.Value;
        // ...
    }
}
```

## Scope Inheritance

**Important:** All fixture dependencies exist in the **same scope** as the dependent fixture.

```csharp
public class SharedStateTests
{
    // Both fixtures share the same Class scope
    protected LogFileFixture LogFile { get; } = 
        TestContext.Current.GetFeffFixture<LogFileFixture>(FixtureScopeType.Class);
    
    // The TmpDirectoryFixture inside LogFileFixture also uses Class scope
    // Even though you didn't specify it explicitly
}
```

### Scope Rules

1. **Same Scope**: Dependencies are created in the same scope as the parent fixture
2. **No Cyclic Dependencies**: Fixtures cannot depend on each other cyclically (A → B → A)
3. **Lazy Initialization**: Fixtures are created only when first requested

## See Also

| Link | Description |
|------|-------------|
| [FixtureDependenciesTests.cs](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/EngineTests/FixtureDependenciesTests.cs) | Core tests for fixture dependency registration and resolution |
