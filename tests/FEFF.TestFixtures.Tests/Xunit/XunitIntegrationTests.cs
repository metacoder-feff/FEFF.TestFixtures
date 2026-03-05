namespace FEFF.TestFixtures.Tests;

//TODO: test DisposeCalled by xunit
//TODO: multiple scopes by xunit

public class XunitIntegrationTests : XunitIntegratedFixtureTestBase
{
    [Fact]
    public void Fixture__should_be_registered_and_returned()
    {
        var f1 = GetFixture<CustomFixture>();

        f1.Value.Should().Be("hello");
    }
}