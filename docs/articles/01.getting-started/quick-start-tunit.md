# Quick Start Guide (TUnit)

This guide walks you through setting up and using FEFF.TestFixtures with TUnit.

## Prerequisites

- .NET 8.0 or later
- TUnit test framework
- Basic knowledge of TUnit testing

## Installation

Add the TUnit integration package to your test project:

```bash
dotnet add package FEFF.TestFixtures.TUnit
```
## Configuration

No assembly-level configuration is required for TUnit. The integration works automatically once the package is installed.

## Your First Test

Here's a complete example using the built-in `TmpDirectoryFixture`:

```csharp
using AwesomeAssertions;
using FEFF.TestFixtures;
using FEFF.TestFixtures.TUnit;

namespace MyProject.Tests;

public class SystemUnderTest
{
    public static void Write(string filePath, string content) =>
        File.WriteAllText(filePath, content, Encoding.UTF8);
}

public class FileTests
{
    // Get the fixture instance from the test context
    protected TmpDirectoryFixture TmpDir { get; } = 
        TestContext.Current!.GetFeffFixture<TmpDirectoryFixture>();

    [Test]
    public async Task File__should_be_created()
    {
        // Arrange
        var filePath = TmpDir.Path + "/file.tmp";
        File.Exists(filePath).Should().BeFalse();

        // Act
        SystemUnderTest.Write(filePath, "some-data");

        // Assert
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath, Encoding.UTF8).Should().Be("some-data");
    }
}
```

### How It Works

1. **Fixture Declaration**: The `TmpDir` property retrieves a `TmpDirectoryFixture` instance from the test context.
2. **Automatic Lifecycle**: The fixture is created on first use (lazy initialization) and automatically disposed after the test completes.
3. **Test Case Scope**: By default, fixtures use the `test-case` scope, meaning a new instance is created for each test method.
4. **Cleanup**: The temporary directory and all its contents are automatically deleted after the test.

## Fixture Scopes

FEFF.TestFixtures supports five scope types for TUnit:

| Scope | Description | Use Case |
|-------|-------------|----------|
| `TestCase` | New fixture for each test method | Default, isolated tests |
| `Class` | One fixture per test class | Share state across tests in a class |
| `Assembly` | One fixture per test assembly | Run once for all tests in the assembly |
| `Session` | One fixture per test session | Run once for all tests |

### Changing the Scope

Use the overloaded method to specify a different scope:

```csharp
protected MyFixture MyFixture { get; } = 
    TestContext.Current!.GetFeffFixture<MyFixture>(FixtureScopeType.Class);
```

## Examples

Full working examples are available in the [GitHub repository](https://github.com/metacoder-feff/FEFF.TestFixtures/tree/main/examples):

- [TUnit Examples](https://github.com/metacoder-feff/FEFF.TestFixtures/tree/main/examples/ExampleTests.TUnit)
- [ASP.NET Core Examples](https://github.com/metacoder-feff/FEFF.TestFixtures/tree/main/examples/ExampleTests.AspNetCore)
