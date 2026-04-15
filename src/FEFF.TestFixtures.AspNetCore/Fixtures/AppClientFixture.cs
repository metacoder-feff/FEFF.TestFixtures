namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Exposes an <see cref="HttpClient"/> connected to the application under test.
/// </summary>
public interface IAppClientFixture
{
    /// <summary>
    /// Gets the lazily-created <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// Starts the application under test on first access if not already running.
    /// </remarks>
    HttpClient LazyValue { get; }
}

/// <summary>
/// This fixture returns <see cref="HttpClient"/> connected to an application being tested.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
[Fixture]
public sealed class AppClientFixture<TEntryPoint> : IDisposable, IAppClientFixture
where TEntryPoint : class
{
    private readonly Lazy<HttpClient> _client;

    /// <inheritdoc/>
    public HttpClient LazyValue => _client.Value;

    /// <summary>
    /// Creates a new <see cref="AppClientFixture{TEntryPoint}"/> that will create an <see cref="HttpClient"/>
    /// when <see cref="LazyValue"/> is first accessed.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    public AppClientFixture(AppManagerFixture<TEntryPoint> app)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _client = new(() => app.LazyApplication.CreateClient());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_client.IsValueCreated)
            _client.Value.Dispose();
    }
}
