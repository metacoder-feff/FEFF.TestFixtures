namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// This fixture returns <see cref="HttpClient"/> connected to an application being tested.
/// </summary>
[Fixture]
public sealed class AppClientFixture : IDisposable
{
    private readonly Lazy<HttpClient> _client;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    public HttpClient Value => _client.Value;

    public AppClientFixture(ITestApplicationFixture app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _client = new(() => app.LazyCreatedApplication.CreateClient());
    }

    public void Dispose()
    {
        if(_client.IsValueCreated)
            _client.Value.Dispose();
    }
}
