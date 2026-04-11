namespace FEFF.TestFixtures.AspNetCore;

public interface IAppServicesFixture
{
    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns ServiceScope.
    /// </summary>
    public IServiceProvider LazyServiceProvider { get; }
}

/// <summary>
/// This fixture allows to get services from the application under test, including scoped services.
/// </summary>
[Fixture]
public sealed class AppServicesFixture<TEntryPoint> : IAsyncDisposable, IAppServicesFixture
where TEntryPoint: class
{
    private readonly Lazy<AsyncServiceScope> _appServiceScope;

    /// <inheritdoc/>
    public IServiceProvider LazyServiceProvider => _appServiceScope.Value.ServiceProvider;

    public AppServicesFixture(AppManagerFixture<TEntryPoint> app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _appServiceScope = new(() => app.LazyApplication.Services.CreateAsyncScope());
    }

    public ValueTask DisposeAsync()
    {
        if(_appServiceScope.IsValueCreated)
            return _appServiceScope.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
