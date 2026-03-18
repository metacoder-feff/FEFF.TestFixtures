namespace FEFF.TestFixtures.Tests;

public class DisposableFixtureTests : FixtureTestBase
{
    [Fact]
    public async Task DisposableFixture__after_scope_ends__should_be_disposed()
    {
        // Arrange
        var f1 = Helper.GetFixture<DisposableFixture>();
        f1.IsDisposed.Should().BeFalse();

        // Act
        await Helper.Scope.DisposeAsync();
        
        // Assert
        f1.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncDisposableFixture__after_scope_ends__should_be_disposed()
    {
        // Arrange
        var f1 = Helper.GetFixture<AsyncDisposableFixture>();
        f1.IsDisposed.Should().BeFalse();

        // Act
        await Helper.Scope.DisposeAsync();
        
        // Assert
        f1.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task BothDisposableFixture__after_scope_ends__should_be_disposed__ASYNC_ONLY()
    {
        // Arrange
        var f1 = Helper.GetFixture<BothDisposableFixture>();
        f1.IsDisposedSync.Should().BeFalse();
        f1.IsDisposedAsync.Should().BeFalse();

        // Act
        await Helper.Scope.DisposeAsync();
        
        // Assert
        f1.IsDisposedSync.Should().BeFalse();       // Only DisposeAsync has been called
        f1.IsDisposedAsync.Should().BeTrue();
    }
    
    [Fact(Skip = "implemented in dotnet 11: https://github.com/dotnet/runtime/commit/800f26a3668eef82327355f9f11bc9ec6ca8ee1f")]
    public async Task Dispose__with_exeception__should_not_prevent_other_fixtures_from_being_disposed()
    {
        //ServiceProviderEngineScope should aggregate exceptions in Dispose rather than throwing on the first
        //[MERGED to main (dotnet-11-preview)] 
        //https://github.com/dotnet/runtime/pull/123342

        // fixtures would be disposed in same/reverse order
        var f1 = Helper.GetFixture<DisposableFixture>();
        _ = Helper.GetFixture<ErrorDisposableFixture>();
        var f2 = Helper.GetFixture<AsyncDisposableFixture>();

        var act = () => Helper.Scope.DisposeAsync().AsTask();
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();

        // assert previous and next
        f1.IsDisposed.Should().BeTrue();
        f2.IsDisposed.Should().BeTrue();
    }
}

[Fixture]
internal class DisposableFixture : IDisposable
{
    public bool IsDisposed { get; private set;}

    public void Dispose() => IsDisposed = true;
}

[Fixture]
internal sealed class AsyncDisposableFixture : IAsyncDisposable
{
    public bool IsDisposed { get; private set;}

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }
}

[Fixture]
internal sealed class BothDisposableFixture : IAsyncDisposable, IDisposable
{
    public bool IsDisposedSync { get; private set;}
    public bool IsDisposedAsync { get; private set;}

    public void Dispose() => IsDisposedSync = true;

    public ValueTask DisposeAsync()
    {
        IsDisposedAsync = true;
        return ValueTask.CompletedTask;
    }
}

[Fixture]
internal class ErrorDisposableFixture : IDisposable
{
    public void Dispose() => throw new InvalidOperationException("test exception");
}