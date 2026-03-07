namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// This fixture allows to get services from an application being tested including scoped services.
/// </summary>
[Fixture]
public sealed class AppServicesFixture : IAsyncDisposable
{
    private readonly Lazy<AsyncServiceScope> _appServiceScope;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns ServiceScope.
    /// </summary>
    public IServiceProvider ServiceProvider => _appServiceScope.Value.ServiceProvider;

    public AppServicesFixture(ITestApplicationFixture app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _appServiceScope = new(() => app.LazyCreatedApplication.Services.CreateAsyncScope());
    }

    public ValueTask DisposeAsync()
    {
        if(_appServiceScope.IsValueCreated)
            return _appServiceScope.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
    
    public T GetRequiredService<T>() where T : notnull =>
        ServiceProvider.GetRequiredService<T>();
}
