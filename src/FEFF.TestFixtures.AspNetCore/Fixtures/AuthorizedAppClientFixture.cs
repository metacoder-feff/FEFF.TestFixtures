namespace FEFF.TestFixtures.AspNetCore.Preview;
//TODO: add test
//TODO: subtype of AppClientFixture??

/// <summary>
/// Defines the contract for configuration options for <see cref="AuthorizedAppClientFixture{TEntryPoint, TOptions}"/>.
/// </summary>
public interface IAuthorizedClientFixtureOptions
{
    /// <summary>
    /// Returns a JWT token used to authenticate requests made by the authorized client.
    /// </summary>
    /// <returns>The JWT token string.</returns>
    string GetJwt();
}

/// <summary>
/// A fixture that provides an <see cref="HttpClient"/> with a Bearer token pre-configured
/// for authenticated requests to the application under test.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
/// <typeparam name="TOptions">The options type implementing <see cref="IAuthorizedClientFixtureOptions"/>.</typeparam>
[Fixture]
public sealed class AuthorizedAppClientFixture<TEntryPoint, TOptions> : IDisposable, IAppClientFixture
where TEntryPoint : class
where TOptions : IAuthorizedClientFixtureOptions
{
    private readonly AppManagerFixture<TEntryPoint> _app;
    private readonly TOptions _opts;
    private readonly Lazy<HttpClient> _client;

    /// <inheritdoc/>
    public HttpClient LazyValue => _client.Value;

    /// <summary>
    /// Creates a new authorized app client fixture.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    /// <param name="opts">The authorization options.</param>
    public AuthorizedAppClientFixture(AppManagerFixture<TEntryPoint> app, TOptions opts)
    {
        _app = app;
        _opts = opts;

        _client = new(CreateClient);
    }

    private HttpClient CreateClient()
    {
        var token = ThrowHelper.EnsureNotNull(
            _opts.GetJwt()
        );

        var res = _app.LazyApplication.CreateClient();
        res.AddBearerHeader(token);

        return res;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_client.IsValueCreated)
            _client.Value.Dispose();
    }
}
