using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.TUnit.Tests;

internal class BaseFix : IDisposable
{
    public void Dispose()
    {
        var name = this.GetType().Name;
        Infrastructure.Add(name);
    }
}

[Fixture]
class TestFix : BaseFix {}
[Fixture]
class ClassFix : BaseFix {}
[Fixture]
class AssemblyFix : BaseFix {}
[Fixture]
class SessionFix : BaseFix {}

class SingletoneFix : BaseFix, IFixureRegistrator
{
    public static void RegisterFixture(IServiceCollection services)
    {
        services.AddSingleton<SingletoneFix>();
    }
}

public class TestSubject
{   
    protected static T GetFixture<T>(FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        return TestContext.Current!.GetFeffFixture<T>(scopeType);
    }

    [Test]
    public void Fixtures__should_be_registered_and_materialized()
    {
        var f1 = GetFixture<TestFix>();
        var f2 = GetFixture<ClassFix>(FixtureScopeType.Class);
        var f4 = GetFixture<AssemblyFix>(FixtureScopeType.Assembly);
        var f5 = GetFixture<SessionFix>(FixtureScopeType.Session);
        var s  = GetFixture<SingletoneFix>();

        f1.Should().BeOfType<TestFix>();
        f2.Should().BeOfType<ClassFix>();
        f4.Should().BeOfType<AssemblyFix>();
        f5.Should().BeOfType<SessionFix>();
        s.Should().BeOfType<SingletoneFix>();
    }

    [Test]
    public void Second_test_method()
    {
        var f1 = GetFixture<TestFix>();
        var f2 = GetFixture<ClassFix>(FixtureScopeType.Class);
        var f4 = GetFixture<AssemblyFix>(FixtureScopeType.Assembly);
        var f5 = GetFixture<SessionFix>(FixtureScopeType.Session);
        var s  = GetFixture<SingletoneFix>();

        f1.Should().BeOfType<TestFix>();
        f2.Should().BeOfType<ClassFix>();
        f4.Should().BeOfType<AssemblyFix>();
        f5.Should().BeOfType<SessionFix>();
        s.Should().BeOfType<SingletoneFix>();
    }
}

[InheritsTests]
public class SecondTestSubject : TestSubject {}