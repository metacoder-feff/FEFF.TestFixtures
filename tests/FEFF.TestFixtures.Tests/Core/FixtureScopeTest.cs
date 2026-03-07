namespace FEFF.TestFixtures.Tests;

//TODO: test RegisterWithType registration error

public class FixtureScopeTest : FixtureScopeTestBase
{
    [Fact]
    public void Fixture__should_be_registered_and_returned()
    {
        var f1 = GetFixture<CustomFixture>();

        f1.Value.Should().Be("hello");
    }

    [Fact]
    public void Fixtures__from_same_scope__should_be_equal()
    {
        var f1 = GetFixture<CustomFixture>();
        var f2 = GetFixture<CustomFixture>();

        f2.Should().Be(f1);
    }

    [Fact]
    public async Task Fixtures__from_diff_scopes__should_NOT_be_equal()
    {
        await using var sc1 = Factory.CreateScope();
        await using var sc2 = Factory.CreateScope();

        var f1 = sc1.GetFixture<CustomFixture>();
        var f2 = sc2.GetFixture<CustomFixture>();

        f2.Should().NotBe(f1);
    }
}

[Fixture]
internal class CustomFixture
{
    public string Value { get; } = "hello";
}