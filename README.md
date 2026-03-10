# FEFF.TestFixures

??? brief

## Contents

???

## Terminology and Goals

A **fixture** is a reusable component used for testing purposes. Fuxures can be packaged into the libraries and reused by any number of testing projects.
The **fixture** is a class containig three optional parts:

+ setup code in constructor;
+ state;
+ teardown code in Dispose() or DisposeAsync().

A **scope** of the fixures defines a lifetime of those fixures. The fixture is created lazyly on demand and destroyed at the end of scope. If the fixture has Dispose() or DisposeAsync() - it is called.
A list of **scopes** is defined by test framework used. For **Xunit Integration** available scopes are:

| Scope name | Description |
| --- | -- |
| test-case | Fixtures are created and destroyed for each test case |
| class | Fixtures are created and destroyed once for each test class |
| collection | Fixtures are created and destroyed once for each test collection (TODO: link) |
| assembly | Fixtures are created and destroyed once for a test assembly |

Every request of the same fixture from same scope results in the same fixture instance. Hence class, collection and assembly **fixtures can share state** between all tests within the same scope.

## Getting started (Xunit)

Add library reference to a test project:

``` bash
dotnet install package ???
```

Add assembly-level attribute to initialize the extension:

``` csharp
[assembly: FEFF.TestFixtures.Xunit.TestFixturesExtension]
```

Use ```TestContext.Current.GetFeffFixture<T>()``` extension method to get required fixture at any moment of a test:

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
The *TmpDir* with it's content would be deleted automatically after the test finishes .

### Difining the scope of a fixture

The scope of a fixture is defined by test creator using overloaded method:

``` csharp
TestContext.Current.GetFeffFixture<T>(FixtureScopeType scopeType)
```

Also note that multiple instanses of the fixture can exist in different scopes if needed.

### Creating a fixture

To create a fixture user has to create a class with ```FixtureAttrubute```. Let's look at sources of ```TmpDirectoryFixture``` we have used above.

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
|Setup              | Constuctor                |
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

## Advanced

...
