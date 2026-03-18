using FEFF.TestFixtures.Core;

namespace FEFF.TestFixtures.Tests;

//TODO: test RegisterWithType registration error

public sealed class FixtureScopeTest : IAsyncDisposable
{
    private T GetFixture<T>()
    where T : notnull
    {
        return Scope.GetFixture<T>();
    }

    private readonly FixtureScopeFactory Factory;
    private readonly FixtureScope Scope;

    public FixtureScopeTest()
    {
        Factory = new FixtureScopeFactory();
        Scope = Factory.CreateScope();
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync().ConfigureAwait(false);;
        await Factory.DisposeAsync().ConfigureAwait(false);;
    }

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