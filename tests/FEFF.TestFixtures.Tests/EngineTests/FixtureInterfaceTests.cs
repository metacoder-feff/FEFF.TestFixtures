using FEFF.TestFixtures.Tests;

namespace FEFF.TestFixtures.Engine.Tests;

public class FixtureInterfaceTests : FixtureTestBase
{
    [Fact]
    public void Fixture__with_interface__should_be_registered_twice()
    {
        // registration by implemetation type
        CustomFixtureWithInterface f1 = Helper.GetFixture<CustomFixtureWithInterface>();
        f1.Value.Should().Be("world");

        // registration by RegisterWithType
        ICustomFixtureInterface f2 = Helper.GetFixture<ICustomFixtureInterface>();
        f2.Value.Should().Be("world");

        // both points to same instance
        f2.Should().Be(f1);
    }

    [Fact]
    public void GetFixture__with_error_interface__should_throw()
    {
        var act = () => _= Helper.GetFixture<IDataAttribute>();

        act.Should().ThrowExactly<InvalidCastException>();
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

// fixture with invalid RegisterWithType
[Fixture(RegisterWithType = typeof(IDataAttribute))]
internal class ErrorFixtureWithInterface
{
    public string Value { get; } = "world";
}