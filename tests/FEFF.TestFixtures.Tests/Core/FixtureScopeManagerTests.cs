namespace FEFF.TestFixtures.Tests;

//TODO: provider disposed (singletones)
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
    public async Task Dispose__should_dispose_nested_scope()
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
    
    [Fact]
    public async Task Dispose__with_exeception__should_not_prevent_other_scopes_from_being_disposed()
    {
        var manager = new FixtureScopeManager();

        // scopes would be disposed in same/reverse order
        var f1 = manager.GetScope("test-1").GetFixture<DisposableFixture>();
        _ = manager.GetScope("test-2").GetFixture<ErrorDisposableFixture>();
        var f2 = manager.GetScope("test-3").GetFixture<AsyncDisposableFixture>();

        var act = () => manager.DisposeAsync().AsTask();
        var err = await act.Should()
            .ThrowExactlyAsync<AggregateException>();
        err.WithInnerExceptionExactly<InvalidOperationException>();

        // assert previous and next
        f1.IsDisposed.Should().BeTrue();
        f2.IsDisposed.Should().BeTrue();
    }
}