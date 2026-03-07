namespace FEFF.TestFixtures.Tests;

public class FixtureScopeManagerTests
{
    [Fact]
    public async Task FixtureScopes__should_be_cached()
    {
        await using var manager = new FixtureScopeManager();

        var sc1 = manager.GetScope("test-1");
        var sc2 = manager.GetScope("test-1");

        sc1.Should().Be(sc2);
    }

    [Fact]
    public async Task FixtureScopes__should_be_distinct()
    {
        await using var manager = new FixtureScopeManager();

        var sc1 = manager.GetScope("test-1");
        var sc2 = manager.GetScope("test-2");

        sc1.Should().NotBe(sc2);
    }

    [Fact]
    public async Task Dispose__should_dispose_scope()
    {
        var manager = new FixtureScopeManager();

        var sc1 = manager.GetScope("test-1");

        var f = sc1.GetFixture<DisposableFixture>();
        f.IsDisposed.Should().BeFalse();

        await manager.DisposeAsync();
        f.IsDisposed.Should().BeTrue();

        // error
        //var f2 = sc1.GetFixture<DisposableFixture>();
    }
}