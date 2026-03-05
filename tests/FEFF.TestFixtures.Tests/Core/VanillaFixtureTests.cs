namespace FEFF.TestFixtures.Tests;

//TODO: test RegisterWithType registration error
//TODO: multiple scopes

public class VanillaFixtureTests : VanillaFixtureTestBase
{
    [Fact]
    public void Fixture__should_be_registered_and_returned()
    {
        var f1 = GetFixture<CustomFixture>();

        f1.Value.Should().Be("hello");
    }
}