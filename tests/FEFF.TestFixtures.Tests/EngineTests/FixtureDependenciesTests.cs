using FEFF.TestFixtures.Tests;

namespace FEFF.TestFixtures.Engine.Tests;

using Subjects;

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

    [Fact]
    public void Transient_assembly_reference__should_be_resolved()
    {
        var f = Helper.GetFixture<RefFixture>();
        f.RefRefValue.Should().Be("123");
    }
}

[Fixture]
internal record CustomFixtureWithDeps(
    CustomFixture F1,
    ICustomFixtureInterface F2 // this interface is already registered by 'CustomFixtureWithInterface'
);