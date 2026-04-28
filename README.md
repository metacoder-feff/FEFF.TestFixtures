# FEFF.TestFixtures

[![Test](https://github.com/metacoder-feff/FEFF.TestFixtures/actions/workflows/test.yml/badge.svg)](https://github.com/metacoder-feff/FEFF.TestFixtures/actions/workflows/test.yml)
[![Release](https://github.com/metacoder-feff/FEFF.TestFixtures/actions/workflows/release-nuget.yml/badge.svg)](https://github.com/metacoder-feff/FEFF.TestFixtures/actions/workflows/release-nuget.yml)

Integrations:
[![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.XunitV3?label=FEFF.TestFixtures.XunitV3)](https://www.nuget.org/packages/FEFF.TestFixtures.XunitV3)
[![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.TUnit?label=FEFF.TestFixtures.TUnit)](https://www.nuget.org/packages/FEFF.TestFixtures.TUnit)  
Fixture libraries:
[![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures?label=FEFF.TestFixtures)](https://www.nuget.org/packages/FEFF.TestFixtures)
[![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.AspNetCore?label=FEFF.TestFixtures.AspNetCore)](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore)
[![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.AspNetCore.EF?label=FEFF.TestFixtures.AspNetCore.EF)](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.EF)
[![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.AspNetCore.SignalR?label=FEFF.TestFixtures.AspNetCore.SignalR)](https://www.nuget.org/packages/FEFF.TestFixtures.AspNetCore.SignalR)

✅ Replace setup/teardown methods and test-class "Disposable pattern" with reusable **Fixtures**.  
✅ Fixtures can depend on other fixtures.  
✅ Fixtures can be configured via standard IServiceProvider.  
✅ Set of fixtures to simplify testing AspNetCore applications [see full AspNetCore example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs).  

[Fixture list](#fixture-list)

[Full Documentation](https://metacoder-feff.github.io/FEFF.TestFixtures/)

## Prerequisites

+ .NET 8.0 or later
+ Test framework: xUnit v3 **or** TUnit
+ For ASP.NET Core fixtures: ASP.NET Core 8.0 or later

## Getting started (Xunit.V3)

Add a library reference to a test project:

```bash
dotnet add package FEFF.TestFixtures.XunitV3
```

Add an assembly-level attribute to initialize the extension:

```csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

Use the `TestContext.Current.GetFeffFixture<T>()` extension method to get the required fixture instance at any point during a test:

```csharp
public class SystemUnderTest
{
    public static void Write(string filePath) =>
        File.WriteAllText(filePath, "some-data", Encoding.UTF8);
}

public class ExampleTests
{
    protected TmpDirectoryFixture TmpDir { get; } = TestContext.Current.GetFeffFixture<TmpDirectoryFixture>();

    [Fact]
    public void File__should_be_created()
    {
        // Arrange
        var filePath = TmpDir.Path + "/file.tmp";
        File.Exists(filePath).Should().BeFalse();

        // Act
        SystemUnderTest.Write(filePath);

        // Assert
        File.Exists(filePath)
            .Should().BeTrue();

        File.ReadAllText(filePath, Encoding.UTF8)
            .Should().Be("some-data");
    }
}
```

In this example, `TmpDir` is created when the fixture is first requested in the test class constructor. The **scope** of the fixture in this example is `test-case`.
The `TmpDir` and its contents are automatically deleted after the test finishes.\
This example uses [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) for the `Should()` assertion DSL.\
See the [source code for this example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs).

### Defining other scopes for a fixture

The scope of a fixture is defined by the test creator using an overloaded method:

```csharp
TestContext.Current.GetFeffFixture<T>(FixtureScopeType scopeType)
```

Also note that multiple instances of the fixture can exist in different scopes if needed.

### Creating a fixture

Just create a class with the `FixtureAttribute`.
Let's examine the source code for the `TmpDirectoryFixture` we used above.

```csharp
[Fixture]
public sealed class TmpDirectoryFixture : IDisposable
{
    public string Path { get; } = Directory.CreateTempSubdirectory().FullName;

    public void Dispose()
    {
        // double dispose guard
        try
        {
            Directory.Delete(Path, true);
        }
        catch (DirectoryNotFoundException)
        {
        }
    }
}
```

### Fixture Dependencies

Fixtures can depend on other fixtures. Dependencies are injected via the constructor:

```csharp
[Fixture]
public class MyCustomFixture1
{
}

[Fixture]
public class MyCustomFixture2
{
}

[Fixture]
public record MyFixtureSet(
    MyCustomFixture1 F1,
    MyCustomFixture2 F2,
);

public class ExampleTests
{
    protected MyFixtureSet Set { get; } = TestContext.Current.GetFeffFixture<MyFixtureSet>();

    //...
}
```

Note:

+ All fixture dependencies (`MyCustomFixture1` and `MyCustomFixture2`) exist in the same scope as the dependent fixture (`MyFixtureSet` in the example above).
+ Fixtures cannot have cyclic dependencies.

## Fixture List

### FEFF.TestFixtures Library

+ EnvironmentFixture
  + Snapshots the process environment and restores it after a test (the test scope)
+ TmpDirectoryFixture
  + Creates a unique directory and removes it along with its contents after a test (the test scope)
  + Can optionally skip deletion
+ TmpScopeIdFixture
  + Generates a string that is unique for each scope (e.g., for each test)

### FEFF.TestFixtures.AspNetCore Fixture Library

[See usage examples](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.AspNetCore/ApiTests.cs)

+ AppManagerFixture
  + Starts and stops the application via TestHost
  + Allows modification of Configuration/ServiceCollection before the application starts
+ AppClientFixture
  + Creates and disposes an HttpClient for testing
+ AppServicesFixture
  + Provides access to the application's service provider in the test context
  + Creates and disposes ServiceScope
+ FakeLoggerFixture
  + Adds FakeLoggerProvider to the application under test
  + Provides access to the application's logs in the test context
+ FakeTimeFixture
  + Replaces TimeProvider with FakeTimeProvider
  + Provides an ability to manipulate application's time
+ FakeRandomFixture
  + Replaces Random with FakeRandom
  + Provides an ability to manipulate random values that application receives
+ TmpDatabaseNameFixture
  + Updates connection strings to use a unique database name for test isolation

### FEFF.TestFixtures.AspNetCore.EF Fixture Library

+ DatabaseLifecycleFixture
  + Removes the database at the end of the fixture scope (e.g., after a test) (DbContext.EnsureDeleted)
  + Can optionally create database at any time of the test (DbContext.EnsureCreated)

### [FEFF.TestFixtures.AspNetCore-Preview]

+ AuthorizedAppClientFixture
  + Creates and disposes an authenticated HttpClient for testing
  + The user must provide a valid JWT
+ SignalrClientFixture
  + Creates and disposes a SignalR client for testing
  + Fixture provides awaitable events for server messages
  + The user must provide a path to the SignalR Hub within the tested API
  + The user can optionally provide a JWT to authenticate the SignalR client

### Development in Progress

The following fixtures are planned but not yet implemented:

+ A unique Redis key prefix for test isolation
+ A unique S3 path prefix for test isolation
+ Fake outbound HTTP connection to stub integrations with third-party APIs

[Full Documentation](https://metacoder-feff.github.io/FEFF.TestFixtures/)
