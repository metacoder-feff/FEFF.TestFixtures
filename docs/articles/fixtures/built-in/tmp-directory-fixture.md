# TmpDirectoryFixture

> **Assembly**: `FEFF.TestFixtures.dll`  
> **Namespace**: `FEFF.TestFixtures`  
> **Source**: [`TmpDirectoryFixture.cs`](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/src/FEFF.TestFixtures/Fixtures/TmpDirectoryFixture.cs)

## Overview

`TmpDirectoryFixture` creates a temporary subdirectory using `Directory.CreateTempSubdirectory()` and ensures its deletion after the test scope finishes. The fixture supports multiple scopes and can be configured to skip deletion for debugging or CI optimization.

## Basic Usage

Retrieve the fixture from the test context:

```csharp
using FEFF.TestFixtures;

public class SystemUnderTest
{
    public static void Write(string filePath) =>
        File.WriteAllText(filePath, "some-data", Encoding.UTF8);
}

public class FileTests
{
    protected TmpDirectoryFixture TmpDir { get; } = 
        TestContext.Current.GetFeffFixture<TmpDirectoryFixture>();

    [Fact]
    public void File__should_be_created()
    {
        // Arrange
        var filePath = Path.Combine(TmpDir.Path, "test.txt");

        // Act
        SystemUnderTest.Write(filePath);

        // Assert
        File.Exists(filePath).Should().BeTrue();
    }
}
```

### Key Members

| Property | Type | Description |
|----------|------|-------------|
| `Path` | `string` | Full path to the temporary directory |


## Configuration

The `TmpDirectoryFixture` supports configuration through the `Microsoft.Extensions.Options` pattern. You can control disposal behavior and directory naming.

### Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `DisposeType` | `DisposeType` | `Delete` | Whether to delete the directory on disposal |
| `Prefix` | `string?` | `null` | Custom prefix for the directory name |

### DisposeType Enum

| Value | Description |
|-------|-------------|
| `Delete` | Deletes the directory and all contents on disposal (default) |
| `Skip` | Preserves the directory after disposal (useful for debugging) |

### Configuration Methods

#### Environment Variables

Set environment variables to configure the fixture:

```bash
# Windows
set TmpDirectoryFixture__DisposeType=Skip
set TmpDirectoryFixture__Prefix=mytest_

# Linux/macOS
export TmpDirectoryFixture__DisposeType=Skip
export TmpDirectoryFixture__Prefix=mytest_
```

## See Also

| Resource | Description |
|----------|-------------|
| [API: TmpDirectoryFixture](xref:FEFF.TestFixtures.TmpDirectoryFixture) | API reference |
| [Basic Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures/TmpDirectoryFixtureTests.cs) | Core functionality test coverage |
| [Options Tests](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/tests/FEFF.TestFixtures.Tests/FixturesTests/FEFF.TestFixtures/TmpDirectoryFixtureOptionsTests.cs) | Configuration and options test coverage |
| [xUnit Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs) | Working example with xUnit v3 |
| [TUnit Example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.TUnit/ExampleTests.cs) | Working example with TUnit |
