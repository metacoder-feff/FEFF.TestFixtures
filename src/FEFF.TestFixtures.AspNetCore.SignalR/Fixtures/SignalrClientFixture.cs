namespace FEFF.TestFixtures.AspNetCore.SignalR;

/// <summary>
/// Defines the contract for configuration options for <see cref="SignalrClientFixture{TEntryPoint, TOptions}"/>.
/// </summary>
public interface ISignalrClientFixtureOptions
{
    /// <summary>
    /// Gets the SignalR hub endpoint path (e.g., "/hub/chat").
    /// </summary>
    string SignalrApiPath { get; }

    /// <summary>
    /// Returns a JWT token for authenticating the SignalR connection, or <c>null</c> for anonymous access.
    /// </summary>
    /// <returns>The JWT token string, or <c>null</c> for anonymous access.</returns>
    string? GetJwt();
}

/// <summary>
/// Exposes a lazily-created <see cref="SignalrTestClient"/> for use in tests.
/// </summary>
public interface ISignalrClientFixture
{
    /// <summary>
    /// Gets the lazily-created SignalR test client.
    /// </summary>
    /// <remarks>
    /// Starting the application being tested occurs on first access to <see cref="AppManagerFixture{TEntryPoint}.LazyApplication"/>.
    /// </remarks>
    SignalrTestClient LazyValue { get; }
}

/// <summary>
/// A fixture that provides a SignalR test client tied to an application managed by
/// <see cref="AppManagerFixture{TEntryPoint}"/>. Creates and disposes the client
/// automatically within the test scope.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
/// <typeparam name="TOptions">The options type implementing <see cref="ISignalrClientFixtureOptions"/>.</typeparam>
[Fixture]
public sealed class SignalrClientFixture<TEntryPoint, TOptions> : IAsyncDisposable, ISignalrClientFixture
where TEntryPoint : class
where TOptions : ISignalrClientFixtureOptions
{
    private readonly AppManagerFixture<TEntryPoint> _app;
    private readonly TOptions _opts;
    private readonly Lazy<SignalrTestClient> _signal;

    /// <inheritdoc/>
    public SignalrTestClient LazyValue => _signal.Value;

    /// <summary>
    /// Creates a new SignalR client fixture.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    /// <param name="opts">The SignalR client configuration options.</param>
    public SignalrClientFixture(AppManagerFixture<TEntryPoint> app, TOptions opts)
    {
        _app = app;
        _opts = opts;

        _signal = new(CreateSignal);
    }

    private SignalrTestClient CreateSignal()
    {
        var token = _opts.GetJwt();
        return _app.LazyApplication.Server.CreateSignalRClient(_opts.SignalrApiPath, token);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (_signal.IsValueCreated)
            return _signal.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
