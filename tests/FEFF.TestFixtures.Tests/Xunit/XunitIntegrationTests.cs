using FEFF.TestFixtures.Xunit;

// register the extension
[assembly: TestFixturesExtension]

namespace FEFF.TestFixtures.Tests;

//TODO: test DisposeCalled multiple scopes by xunit

public class XunitIntegrationTests
{   
    protected static T GetFixture<T>(FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        return TestContext.Current.GetFeffFixture<T>(scopeType);
    }

    [Fact]
    public void Fixture__should_be_registered_and_returned()
    {
        var f1 = GetFixture<CustomFixture>();

        f1.Value.Should().Be("hello");
    }

    [Fact]
    public void TestGetClassFixtures()
    {
        var f = GetFixture<CustomFixture>(FixtureScopeType.Class);
    }

    [Fact]
    public void TestGetCollectionFixtures()
    {
        var f = GetFixture<CustomFixture>(FixtureScopeType.Collection);
    }

    [Fact]
    public void TestGetAssemblyFixtures()
    {
        var f = GetFixture<CustomFixture>(FixtureScopeType.Assembly);
    }
}