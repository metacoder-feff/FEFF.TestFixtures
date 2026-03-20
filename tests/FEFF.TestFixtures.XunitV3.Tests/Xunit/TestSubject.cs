using FEFF.TestFixtures.Xunit;

// register the extension
[assembly: TestFixturesExtension]

namespace FEFF.TestFixtures.Tests;

internal class BaseFix : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        //wait prev scopes to send finish messages
        await Task.Delay(50);

        var ctx = TestContext.Current;
        var name = this.GetType().Name;
        ctx.SendDiagnosticMessage("disposed {0}", name);
    }
}

[Fixture]
class TestFix : BaseFix {}
[Fixture]
class ClassFix : BaseFix {}
[Fixture]
class CollectionFix : BaseFix {}
[Fixture]
class AssemblyFix : BaseFix {}

public class TestSubject
{   
    protected static T GetFixture<T>(FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        return TestContext.Current.GetFeffFixture<T>(scopeType);
    }

    [Fact]
    public void Fixtures__should_be_registered_and_materialized()
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