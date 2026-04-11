namespace FEFF.TestFixtures.AspNetCore.Preview;

public interface ISignalrClientFixtureOptions
{
    string SignalrApiPath { get; }
    // optional!!
    string? GetJwt();
}

public interface ISignalrClientFixture
{
    SignalrTestClient LazyValue { get; }
}

[Fixture]
public sealed class SignalrClientFixture<TEntryPoint, TOptions> : IAsyncDisposable, ISignalrClientFixture 
where TEntryPoint : class
where TOptions : ISignalrClientFixtureOptions
{
    private readonly AppManagerFixture<TEntryPoint> _app;
    private readonly TOptions _opts;
    private readonly Lazy<SignalrTestClient> _signal;

    public SignalrTestClient LazyValue => _signal.Value;

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

    public ValueTask DisposeAsync()
    {
        if (_signal.IsValueCreated)
            return _signal.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}