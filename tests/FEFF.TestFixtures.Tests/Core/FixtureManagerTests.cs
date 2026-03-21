namespace FEFF.TestFixtures.Tests;
using Core;

/// <remarks>
/// Do not use TestFixtures to test <see cref="FixtureManager"/> here 
/// because error in <see cref="FixtureManager"/> or integration would fail everything
/// <remarks/>
public sealed class FixtureManagerTests : IAsyncDisposable 
{
    private readonly FixtureManager manager = new FixtureManagerBuilder().Build();
    public ValueTask DisposeAsync()
    {
        return manager.DisposeAsync();
    }

    [Fact]
    public async Task FixtureScopes__should_be_cached()
    {
        var sc1 = manager.GetScope("test-1");
        var sc2 = manager.GetScope("test-1");

        sc1.Should().Be(sc2);
    }

    [Fact]
    public async Task FixtureScopes__should_be_distinct()
    {
        var sc1 = manager.GetScope("test-1");
        var sc2 = manager.GetScope("test-2");

        sc1.Should().NotBe(sc2);
    }

    [Fact]
    public async Task Dispose__should_dispose_nested_scope()
    {
        var sc1 = manager.GetScope("test-1");

        var f = sc1.GetFixture<DisposableFixture>();
        f.IsDisposed.Should().BeFalse();

        await manager.DisposeAsync();
        f.IsDisposed.Should().BeTrue();

        // error
        //var f2 = sc1.GetFixture<DisposableFixture>();
    }
    
    [Fact]
    public async Task Dispose__with_exception__should_not_prevent_other_scopes_from_being_disposed()
    {
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
    public async Task Dispose__with_MULTIPLE_exceptions__should_throw_AggregateException()
    {
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
    public async Task Dispose__with_SINGLE_exception__should_throw_InvalidOperationException()
    {
        _ = manager.GetScope("test-1").GetFixture<ErrorDisposableFixture>();

        var act = () => manager.DisposeAsync().AsTask();
        var err = await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        err.Which.Message.Should().Be("test exception");
    }
}