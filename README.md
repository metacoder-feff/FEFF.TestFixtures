# FEFF.TestFixtures

![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.XunitV3?label=FEFF.TestFixtures.XunitV3)
![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures?label=FEFF.TestFixtures)

✅ Replace setup/teardown methods and cumbersome "Disposable pattern" with reusable **Fixtures**.  
✅ Fixtures can depend on other fixtures.

## Terminology and Goals

A **fixture** is a reusable component used for testing purposes. Fixtures can be packaged into libraries and reused by any number of testing projects.
The **fixture** is a class containing three optional parts:

+ setup code in constructor;
+ state;
+ teardown code in Dispose() or DisposeAsync().

A **scope** of fixtures defines the lifetime of those fixtures. Within a scope, each fixture is created only once (lazily on demand) and destroyed at the end of the scope. If the fixture has Dispose() or DisposeAsync(), it is called.

The available scopes are defined by the test framework used. For **Xunit Integration**, they are:

| Scope name | Description |
| --- | -- |
| test-case | Fixtures are created and destroyed for each test case |
| class | Fixtures are created and destroyed once for each test class |
| collection | Fixtures are created and destroyed once for each [test collection](https://xunit.net/docs/running-tests-in-parallel#test-collections) |
| assembly | Fixtures are created and destroyed once for a test assembly |

Every request for the same fixture within the same scope returns the same fixture instance. Therefore, class-, collection-, and assembly-level **fixtures can share state** between all tests within the same scope.

## Getting started (Xunit.V3)

Add library reference to a test project:

``` bash
dotnet add package FEFF.TestFixtures.XunitV3
```

Add assembly-level attribute to initialize the extension:

``` csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

Use ```TestContext.Current.GetFeffFixture<T>()``` extension method to get the required fixture instance at any moment of a test:

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

In this example, a *TmpDir* is created once the fixture is requested in the test class constructor. The **scope** of the fixture in this example is 'test-case'.
The *TmpDir* and its contents are automatically deleted after the test finishes.\
This example uses [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions) for ```Should()```.\
Have a look at [source code for this example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs).

## Advanced usage

### Defining other scopes for a fixture

The scope of a fixture is defined by the test creator using an overloaded method:

``` csharp
TestContext.Current.GetFeffFixture<T>(FixtureScopeType scopeType)
```

Also note that multiple instances of the fixture can exist in different scopes if needed.

### Creating a fixture

Create a class with the ```FixtureAttribute```. Let's look at sources of the ```TmpDirectoryFixture``` we used above.

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

Where

|Fixture function   | Implementation            |
|---                | ---                       |
|Setup              | Constructor               |
|State              | 'Path' property           |
|Teardown           | IDisposable               |

### Fixture dependencies

Fixtures can depend on other fixtures. Dependencies are injected via constructor:

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
+ Fixtures can't have cyclic dependencies.

## AspNetCore Fixture Library (preview)

+ Start/Stop application via TestHost and create HttpClient.
+ Use application service provider in the test context.
+ Edit application Configuration/ServiceCollection before starting the application.
+ Set unique (random) database name.
+ Stub TimeProvider by FakeTimeProvider.
+ Stub Random.Shared by FakeRandom.
