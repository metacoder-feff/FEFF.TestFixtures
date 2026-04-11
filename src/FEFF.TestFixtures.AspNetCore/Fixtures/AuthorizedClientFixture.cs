namespace FEFF.TestFixtures.AspNetCore.Preview;
//TODO: add test

public interface IAuthorizedClientFixtureOptions
{
    string GetJwt();
}

[Fixture]
public sealed class AuthorizedAppClientFixture<TEntryPoint, TOptions> : IAsyncDisposable, IAppClientFixture
where TEntryPoint : class
where TOptions : IAuthorizedClientFixtureOptions
{
    private readonly AppManagerFixture<TEntryPoint> _app;
    private readonly TOptions _opts;
    private readonly Lazy<HttpClient> _client;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    public HttpClient LazyValue => _client.Value;

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

    public ValueTask DisposeAsync()
    {
        if(_client.IsValueCreated)
            _client.Value.Dispose();
            
        return ValueTask.CompletedTask;
    }
}