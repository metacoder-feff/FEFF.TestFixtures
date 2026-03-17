# FEFF.TestFixtures

![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures.XunitV3?label=FEFF.TestFixtures.XunitV3)
![NuGet Version](https://img.shields.io/nuget/v/FEFF.TestFixtures?label=FEFF.TestFixtures)

Replace setup/teardown methods with reusable **Fixtures**.
Fixtures can depend on other ones. (!!!)

## Terminology and Goals

A **fixture** is a reusable component used for testing purposes. Fixtures can be packaged into libraries and reused by any number of testing projects.
The **fixture** is a class containing three optional parts:

+ setup code in constructor;
+ state;
+ teardown code in Dispose() or DisposeAsync().

A **scope** of the fixtures defines a lifetime of those fixtures. For a scope, the fixture is created only once lazily on demand and destroyed at the end of the scope. If the fixture has Dispose() or DisposeAsync(), it is called.
A list of **scopes** is defined by the test framework used. For **Xunit Integration** available scopes are:

| Scope name | Description |
| --- | -- |
| test-case | Fixtures are created and destroyed for each test case |
| class | Fixtures are created and destroyed once for each test class |
| collection | Fixtures are created and destroyed once for each [test collection](https://xunit.net/docs/running-tests-in-parallel#test-collections) |
| assembly | Fixtures are created and destroyed once for a test assembly |

Every request of the same fixture from the same scope results in the same fixture instance. Hence class-, collection- and assembly- **fixtures can share state** between all tests within the same scope.

## Getting started (Xunit)

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

In this example a *TmpDir* is created once the fixture is requested at the test class constructor. The **scope** of the fixture in the example is '*test-case*'.
The *TmpDir* with its content would be deleted automatically after the test finishes.\
This example uses [AwesomeAssertions](https://github.com/AwesomeAssertions/AwesomeAssertions)  for:  ```Should()```.\
Have a look at [source code for this example](https://github.com/metacoder-feff/FEFF.TestFixtures/blob/main/examples/ExampleTests.XunitV3/ExampleTests.cs).

## Advanced usage

### Defining other scopes for a fixture

The scope of a fixture is defined by test creator using overloaded method:

``` csharp
TestContext.Current.GetFeffFixture<T>(FixtureScopeType scopeType)
```

Also note that multiple instances of the fixture can exist in different scopes if needed.

### Creating a fixture

To create a fixture, user has to create a class with ```FixtureAttribute```. Let's look at sources of ```TmpDirectoryFixture``` we have used above.

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

+ All fixture dependencies (```MyCustomFixture1``` & ```MyCustomFixture2```) exist in the same scope as the dependent fixture (```MyFixtureSet``` in the example above).
+ Fixtures can't have cyclic dependencies.
