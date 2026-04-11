namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Exposes a lazily-created service scope for resolving services from the application under test.
/// </summary>
public interface IAppServicesFixture
{
    /// <summary>
    /// Gets service provider from a <see cref="IServiceScope"/> of the application under test.
    /// </summary>
    /// <remarks>
    /// Starts the application under test if not already running.
    /// </remarks>
    public IServiceProvider LazyServiceProvider { get; }
}

/// <summary>
/// This fixture allows to get services from the application under test, including scoped services.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
[Fixture]
public sealed class AppServicesFixture<TEntryPoint> : IAsyncDisposable, IAppServicesFixture
where TEntryPoint: class
{
    private readonly Lazy<AsyncServiceScope> _appServiceScope;

    /// <inheritdoc/>
    public IServiceProvider LazyServiceProvider => _appServiceScope.Value.ServiceProvider;

    /// <summary>
    /// Creates a new <see cref="AppServicesFixture{TEntryPoint}"/> that will create an async service scope
    /// when <see cref="LazyServiceProvider"/> is first accessed.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    public AppServicesFixture(AppManagerFixture<TEntryPoint> app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _appServiceScope = new(() => app.LazyApplication.Services.CreateAsyncScope());
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if(_appServiceScope.IsValueCreated)
            return _appServiceScope.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
