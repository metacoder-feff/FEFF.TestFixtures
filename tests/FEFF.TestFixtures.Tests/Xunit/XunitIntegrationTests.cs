using FEFF.TestFixtures.Xunit;

namespace FEFF.TestFixtures.Tests;

//TODO: test DisposeCalled by xunit
//TODO: multiple scopes by xunit
//TODO: better error message when extension is not regitered

public class XunitIntegrationTests : XunitIntegratedFixtureTestBase
{
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
}