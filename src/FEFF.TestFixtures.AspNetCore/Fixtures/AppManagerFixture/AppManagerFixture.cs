using Microsoft.AspNetCore.TestHost;

namespace FEFF.TestFixtures.AspNetCore;

public interface ITestApplication
{
    IServiceProvider    Services    { get; }
    TestServer          Server      { get; } // for signal-r

    HttpClient CreateClient();
}

public interface IAppConfigurator
{
    void ConfigureWebHost(Action<IWebHostBuilder> action);
}

public interface IAppManagerFixture
{
    IAppConfigurator    ConfigurationBuilder    { get; }
    ITestApplication    LazyApplication         { get; }
    bool                IsStarted               { get; }

//TODO: IsStartRequested property (may not be finished)
//TODO: StartAsync & throw at LazyApplication property
}

/// <summary>
/// Allows to configure and start the tested application via <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>
/// </summary>
/// <typeparam name="TEntryPoint">
/// A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.
/// </typeparam>
[Fixture]
public sealed class AppManagerFixture<TEntryPoint> : IAsyncDisposable, IAppManagerFixture
where TEntryPoint: class
{
    // 'Start()' may throw, therefore store _factory for disposing.
    private readonly Lazy<IWebApplicationFactory> _factory;
    private readonly Lazy<ITestApplication> _app;
    private readonly OneTimeAppBuilder<TEntryPoint> _builder = new();

    public IAppConfigurator ConfigurationBuilder => _builder;
    private bool _isDisposed;

    public bool IsStarted => _app.IsValueCreated;

    /// <summary>
    /// Creates, memoizes and returns App.<br/>
    /// Access to <see cref="LazyApplication"/> finishes app building and starts App.
    /// </summary>
    public ITestApplication LazyApplication => _app.Value;

    public AppManagerFixture()
    {
        // factory is created only when _app is creating
        _factory = new (_builder.Build, LazyThreadSafetyMode.None);
        _app = new (GetStartedApp);
    }

    private ITestApplication GetStartedApp()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        
        // OnStarting?.Invoke();

        // build factory
        var res = _factory.Value;

        //start
        res.StartServer();
        return res;
        
        // OnStarted?.Invoke();
        // event handler wants to use the 'LazyApplication' property
        // therefore we have to fire an event after '_app' field finishes initialization
        // see 'LazyApplication' property
    }

    public ValueTask DisposeAsync()
    {
        _isDisposed = true;

        if (_factory.IsValueCreated)
            return _factory.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}

internal class OneTimeAppBuilder<TEntryPoint>: IAppConfigurator
where TEntryPoint: class
{
    private readonly ApplicationBuilder<TEntryPoint> _builder = new();
    private IWebApplicationFactory? _instance;
    internal bool IsBuilt => _instance != null;

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        if(IsBuilt)
            throw new InvalidOperationException($"Can't use '{nameof(IAppConfigurator)}' after application is created.");

        _builder.ConfigureWebHost(action);
    }

    public IWebApplicationFactory Build()
    {
        _instance ??= _builder.Build();
        return _instance;
    }
}