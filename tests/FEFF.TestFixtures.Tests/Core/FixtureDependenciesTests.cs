namespace FEFF.TestFixtures.Tests;

public class FixtureDependenciesTests : FixtureTestBase
{
    [Fact]
    public void Fixture__with_dependencies__should_be_registered_and_returned()
    {
        var f = Helper.GetFixture<CustomFixtureWithDeps>();
        f.F1.Value.Should().Be("hello");
        f.F2.Value.Should().Be("world");
        f.F2.Should().BeOfType<CustomFixtureWithInterface>();
    }
}

[Fixture]
internal record CustomFixtureWithDeps(
    CustomFixture F1,
    ICustomFixtureInterface F2 // this interface is already registered by 'CustomFixtureWithInterface'
);