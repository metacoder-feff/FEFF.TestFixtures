# FEFF.TestFixtures

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

## Terminology and Goals

A **fixture** is a reusable component used for testing purposes. Fixtures can be packaged into libraries and reused by any number of testing projects.
The **fixture** is a class containing three optional parts:

+ Setup code in the constructor;
+ State;
+ Teardown code in Dispose() or DisposeAsync().

The **scope** of a fixture defines its lifetime. Within a scope, each fixture is created only once (lazily on demand) and destroyed at the end of the scope. If the fixture implements Dispose() or DisposeAsync(), those methods are called.

The available scopes are defined by the test framework used. For **Xunit Integration**, they are:

| Scope name | Description |
| --- | -- |
| test-case | Fixtures are created and destroyed for each test case |
| class | Fixtures are created and destroyed once for each test class |
| collection | Fixtures are created and destroyed once for each [test collection](https://xunit.net/docs/running-tests-in-parallel#test-collections) |
| assembly | Fixtures are created and destroyed once for a test assembly |

Every request for the same fixture within the same scope returns the same fixture instance. Therefore, class-, collection-, and assembly-level **fixtures can share state** between all tests within the same scope.

## Getting started (Xunit.V3)

Add a library reference to a test project:

``` bash
dotnet add package FEFF.TestFixtures.XunitV3
```

Add an assembly-level attribute to initialize the extension:

``` csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

Use the ```TestContext.Current.GetFeffFixture<T>()``` extension method to get the required fixture instance at any point during a test:

``` csharp
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

In this example, *TmpDir* is created when the fixture is first requested in the test class constructor. The **scope** of the fixture in this example is 'test-case'.
The *TmpDir* and its contents are automatically deleted after the test finishes.\
This example uses [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) for the ```Should()``` assertion DSL.\
See the [source code for this example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs).

### Defining other scopes for a fixture

The scope of a fixture is defined by the test creator using an overloaded method:

``` csharp
TestContext.Current.GetFeffFixture<T>(FixtureScopeType scopeType)
```

Also note that multiple instances of the fixture can exist in different scopes if needed.

### Creating a fixture

Just create a class with the ```FixtureAttribute```.  
Let's examine the source code for the ```TmpDirectoryFixture``` we used above.

``` csharp
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

Where:

| Fixture function  | Implementation            |
|---                | ---                       |
|Setup              | Constructor               |
|State              | 'Path' property           |
|Teardown           | IDisposable               |

### Fixture Dependencies

Fixtures can depend on other fixtures. Dependencies are injected via the constructor:

``` csharp
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

+ All fixture dependencies (```MyCustomFixture1``` and ```MyCustomFixture2```) exist in the same scope as the dependent fixture (```MyFixtureSet``` in the example above).
+ Fixtures cannot have cyclic dependencies.

## Advanced usage

### Add/Get Fixture by Interface

Documentation is currently under development. [See examples](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/d4e7561bd6bf0a3882e6f2777f0012c4ef9c3aa9/tests/FEFF.TestFixtures.Tests/Core/FixtureInterfaceTests.cs#L6).

### Fixture Factory Internals

Documentation is currently under development. [See examples](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/d4e7561bd6bf0a3882e6f2777f0012c4ef9c3aa9/src/FEFF.TestFixtures/Core/FixtureManager.cs#L14).

### Advanced Fixture Registration

Documentation is currently under development. [See examples](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/ecb983deb9af95dea222037e237e8fc08a4e9c1a/src/FEFF.TestFixtures/Fixtures/TmpDirectoryFixture.cs#L18).

### Configuring Fixtures

Documentation is currently under development. [See examples](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/ecb983deb9af95dea222037e237e8fc08a4e9c1a/src/FEFF.TestFixtures/Fixtures/TmpDirectoryFixture.cs#L38).

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
  + Replaces Logger<> with FakeLogger
+ FakeTimeFixture
  + Replaces TimeProvider with FakeTimeProvider
+ FakeRandomFixture
  + Replaces Random with FakeRandom
+ TmpDatabaseNameFixture
  + Updates connection strings to use a unique database name for test isolation

### FEFF.TestFixtures.AspNetCore.EF Fixture Library

+ DatabaseLifecycleFixture
  + Creates the database on test application start (DbContext.EnsureCreated)
  + Removes the database at the end of the fixture scope (e.g., after a test) (DbContext.EnsureDeleted)

### [FEFF.TestFixtures.AspNetCore-Preview]

+ AuthorizedAppClientFixture
  + Creates and disposes an authenticated HttpClient for testing
  + The user must provide a valid JWT
+ SignalrClientFixture
  + Creates and disposes a SignalR client for testing
  + Fixture provides awaitable events for server messages
  + The user must provide a path to the SignalR Hub within the tested API
  + The user can optionally provide a JWT to authenticate the SignalR client

### [Development in Progress]

+ A unique Redis key prefix for test isolation
+ A unique S3 path prefix for test isolation
+ Fake outbound HTTP connection to stub integrations with third-party APIs
