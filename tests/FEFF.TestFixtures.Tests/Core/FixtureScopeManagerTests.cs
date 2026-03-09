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
        var err = await act.Should().ThrowAsync<Exception>(); // do not check ex type, see other tests

        // assert previous and next
        f1.IsDisposed.Should().BeTrue();
        f2.IsDisposed.Should().BeTrue();
    }
    
    [Fact]
    public async Task Dispose__with_MULTIPLE_execeptions__should_throw_AggregateException()
    {
        var manager = new FixtureScopeManager();
        _ = manager.GetScope("test-1").GetFixture<ErrorDisposableFixture>();
        _ = manager.GetScope("test-2").GetFixture<ErrorDisposableFixture>();

        var act = () => manager.DisposeAsync().AsTask();
        var err = await act.Should().ThrowExactlyAsync<AggregateException>();

        err.Which.InnerExceptions.Should().AllSatisfy( inner =>
            inner.Should()
                .BeOfType<InvalidOperationException>()
                .Which.Message.Should()
                    .Be("test exception")
        );
    }
    
    [Fact]
    public async Task Dispose__with_SINGLE_exeception__should_throw_InvalidOperationException()
    {
        var manager = new FixtureScopeManager();
        _ = manager.GetScope("test-1").GetFixture<ErrorDisposableFixture>();

        var act = () => manager.DisposeAsync().AsTask();
        var err = await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        err.Which.Message.Should().Be("test exception");
    }
}