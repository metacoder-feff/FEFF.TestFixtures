using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using FEFF.TestFixtures.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.v3;

// register the extension
[assembly: TestFixturesExtension]

namespace FEFF.TestFixtures.XunitV4.TestSubjects;

internal class BaseFix : IDisposable
{
    private readonly string? _testName;
    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public BaseFix()
    {
        // for simplier test
        if(GetType() == typeof(AssemblyFix) || GetType() == typeof(SingletonTester))
        {
            _testName = "{}";
            return;
        }

        var tst = TestContext.Current.TestMethod?.MethodName;
        var cls = TestContext.Current.TestClass?.TestClassName;
        var col = TestContext.Current.TestCollection?.TestCollectionDisplayName;

        // for simplier test
        if(cls != null)
            cls = cls.Replace("FEFF.TestFixtures.XunitV4.TestSubjects.", "");

        // remove default collection name for simplier test
        if (col != null && col.StartsWith("Test collection for "))
            col = "";

        // first tst/cls may vary beacuse of parallel execution
        if(GetType() == typeof(ClassFix))
            tst = null;
        if(GetType() == typeof(CollectionFix))
        {
            tst = null;
            cls = null;
        }

        _testName = JsonSerializer.Serialize(
            options: _options,
            value: new
            {
                Test = tst,
                Collection = col,
                Class = cls,
            })
            .Replace("\"", "'") // for test convenience
            ;

    }

    public void Dispose()
    {
        var name = this.GetType().Name;
        Infrastructure.Add($"{name}:{_testName}");
    }
}

[Fixture]
class TestFix : BaseFix { }
[Fixture]
class ClassFix : BaseFix { }
[Fixture]
class CollectionFix : BaseFix { }
[Fixture]
class AssemblyFix : BaseFix { }

class SingletonTester : BaseFix, IFixtureRegistrar
{
    public static bool IsDisposed { get; set; }

    public static void RegisterFixture(IServiceCollection services)
    {
        services.AddSingleton<SingletonTester>();
    }
}

public class TestSubject
{
    protected static T GetFixture<T>(FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        return TestContext.Current.GetFeffFixture<T>(scopeType);
    }

    [Fact]
    //public void Fixtures__should_be_registered_and_materialized()
    public void TestMethod_1()
    {
        var f1 = GetFixture<TestFix>();
        var f2 = GetFixture<ClassFix>(FixtureScopeType.Class);
        var f3 = GetFixture<CollectionFix>(FixtureScopeType.Collection);
        var f4 = GetFixture<AssemblyFix>(FixtureScopeType.Assembly);
        var s0 = GetFixture<SingletonTester>();

        f1.Should().BeOfType<TestFix>();
        f2.Should().BeOfType<ClassFix>();
        f3.Should().BeOfType<CollectionFix>();
        f4.Should().BeOfType<AssemblyFix>();
        s0.Should().BeOfType<SingletonTester>();
    }

    [Fact]
    // public void Second_test_method()
    public void TestMethod_2()

    {
        var f1 = GetFixture<TestFix>();
        var f2 = GetFixture<ClassFix>(FixtureScopeType.Class);
        var f3 = GetFixture<CollectionFix>(FixtureScopeType.Collection);
        var f4 = GetFixture<AssemblyFix>(FixtureScopeType.Assembly);

        f1.Should().BeOfType<TestFix>();
        f2.Should().BeOfType<ClassFix>();
        f3.Should().BeOfType<CollectionFix>();
        f4.Should().BeOfType<AssemblyFix>();
    }
}

[Collection("collecion-a")]
public class SecondTestSubject : TestSubject { }
[Collection("collecion-a")]
public class ThirdTestSubject : TestSubject { }
