namespace FEFF.TestFixtures.Tests;

// TODO: global dispose/scope dispose
public class DisposableFixtureTests : VanillaFixtureTestBase
{
    [Fact]
    public async Task DisposableFixture__after_scope_ends__should_be_disposed()
    {
        // Arrange
        var f1 = GetFixture<DisposableFixture>();
        f1.IsDisposed.Should().BeFalse();

        // Act
        await Container.DisposeAsync();
        
        // Assert
        f1.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncDisposableFixture__after_scope_ends__should_be_disposed()
    {
        // Arrange
        var f1 = GetFixture<AsyncDisposableFixture>();
        f1.IsDisposed.Should().BeFalse();

        // Act
        await Container.DisposeAsync();
        
        // Assert
        f1.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task BothDisposableFixture__after_scope_ends__should_be_disposed__ASYNC_ONLY()
    {
        // Arrange
        var f1 = GetFixture<BothDisposableFixture>();
        f1.IsDisposedSync.Should().BeFalse();
        f1.IsDisposedAsync.Should().BeFalse();

        // Act
        await Container.DisposeAsync();
        
        // Assert
        f1.IsDisposedSync.Should().BeFalse();       // Only DisposeAsync has been called
        f1.IsDisposedAsync.Should().BeTrue();
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