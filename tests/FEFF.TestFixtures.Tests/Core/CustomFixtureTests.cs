namespace FEFF.TestFixtures.Tests;

public class CustomFixtureTests : XunitIntegratedFixtureTestBase
{
    [Fact]
    public void Fixture__with_interface__should_be_registered_twice()
    {
        // registration by implemetation type
        CustomFixtureWithInterface f1 = GetFixture<CustomFixtureWithInterface>();
        f1.Value.Should().Be("world");

        // registration by RegisterWithType
        ICustomFixtureInterface f2 = GetFixture<ICustomFixtureInterface>();
        f2.Value.Should().Be("world");

        // both points to same instance
        f2.Should().Be(f1);
    }
    
    [Fact]
    public void Fixture__with_dependencies__should_be_registered_and_returned()
    {
        var f = GetFixture<CustomFixtureWithDeps>();
        f.F1.Value.Should().Be("hello");
        f.F2.Value.Should().Be("world");
        f.F2.Should().BeOfType<CustomFixtureWithInterface>();
    }
}

internal interface ICustomFixtureInterface
{
    public string Value { get; }
}

[Fixture(RegisterWithType = typeof(ICustomFixtureInterface))]
internal class CustomFixtureWithInterface: ICustomFixtureInterface
{
    public string Value { get; } = "world";
}

[Fixture]
internal record CustomFixtureWithDeps(
    CustomFixture F1,
    ICustomFixtureInterface F2 // this interface is already registered by 'CustomFixtureWithInterface'
);