# Creating Custom Fixtures

## Fixture Structure

A fixture consists of three optional parts:

| Part | Location | Purpose |
|------|----------|---------|
| **Setup** | Constructor | Initialize resources |
| **State** | Properties/Fields | Expose data to tests |
| **Teardown** | `Dispose()` / `DisposeAsync()` | Clean up resources |

## Example: Building TmpDirectoryFixture

Let's create a fixture that provides a unique temporary directory for each test scope and automatically cleans it up afterward.

### Step 1: Basic Structure

Start with the minimal fixture structure:

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

**Key elements:**
- `[Fixture]` attribute - Marks the class for discovery by the framework
- `Path` property - Exposes the directory path to tests
- Constructor - Creates the temporary directory (Setup)

### Step 2: Add Teardown

Implement `IDisposable` to clean up the directory:

```csharp
[Fixture]
public class TmpDirectoryFixture : IDisposable
{
    public string Path { get; }

    public TmpDirectoryFixture()
    {
        Path = Directory.CreateTempSubdirectory().FullName;
    }

    public void Dispose()
    {
        Directory.Delete(Path, true);
    }
}
```

> **Note:** The framework also supports `IAsyncDisposable` for fixtures requiring asynchronous cleanup. Implement `DisposeAsync()` instead of `Dispose()` when your teardown logic involves async operations (e.g., database connections, network calls, file I/O).

```csharp
    public async ValueTask DisposeAsync()
    {
        // Async cleanup logic here
        await Task.Delay(10); // Example: upload logs before deletion
        Directory.Delete(Path, true);
    }
```

### Step 3: Add Error Handling

Protect against disposal errors:

```csharp
[Fixture]
public class TmpDirectoryFixture : IDisposable
{
    public string Path { get; }

    public TmpDirectoryFixture()
    {
        Path = Directory.CreateTempSubdirectory().FullName;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (DirectoryNotFoundException)
        {
            // Directory already deleted - ignore
        }
    }
}
```

## Reference: TmpDirectoryFixture Source and Tests

For a complete working example, see the production implementation and tests:

| Resource | Description |
|----------|-------------|
| [Source Code](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures/Fixtures/TmpDirectoryFixture.cs) | Full implementation of `TmpDirectoryFixture` with options support |
| [Unit Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures/TmpDirectoryFixtureTests.cs) | Comprehensive test coverage |
| [Options Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures/TmpDirectoryFixtureOptionsTests.cs) | Configuration and options usage examples |