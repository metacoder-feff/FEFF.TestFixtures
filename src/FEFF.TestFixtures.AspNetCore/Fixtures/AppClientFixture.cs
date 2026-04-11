namespace FEFF.TestFixtures.AspNetCore;

public interface IAppClientFixture
{
    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    HttpClient LazyValue { get; }
}

/// <summary>
/// This fixture returns <see cref="HttpClient"/> connected to an application being tested.
/// </summary>
[Fixture]
public sealed class AppClientFixture<TEntryPoint> : IDisposable, IAppClientFixture
where TEntryPoint: class
{
    private readonly Lazy<HttpClient> _client;

    /// <inheritdoc/>
    public HttpClient LazyValue => _client.Value;

    public AppClientFixture(AppManagerFixture<TEntryPoint> app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _client = new(() => app.LazyApplication.CreateClient());
    }

    public void Dispose()
    {
        if(_client.IsValueCreated)
            _client.Value.Dispose();
    }
}
