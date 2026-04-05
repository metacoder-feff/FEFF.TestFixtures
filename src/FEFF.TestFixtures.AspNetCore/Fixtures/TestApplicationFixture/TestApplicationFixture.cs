namespace FEFF.TestFixtures.AspNetCore;

public interface ITestApplication
{
    IServiceProvider Services { get; }

    HttpClient CreateClient();
}

public interface IApplicationConfigurator
{
    void ConfigureWebHost(Action<IWebHostBuilder> action);
}

public interface ITestApplicationFixture
{
    IApplicationConfigurator Configuration   { get; }
    ITestApplication         LazyApplication { get; }
    bool                     IsStarted       { get; }
    
//TODO: IsStartRequested property (may not be finished)
//TODO: StartAsync & throw at LazyApplication property
//TODO: Event OnStaring (Async)
//TODO: Event OnStarted (Async)
}

/// <summary>
/// Allows to configure and start tested application via <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>
/// </summary>
/// <typeparam name="TEntryPoint">
/// A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.
/// </typeparam>
[Fixture]
public sealed class TestApplicationFixture<TEntryPoint> : IAsyncDisposable, ITestApplicationFixture
where TEntryPoint: class
{
    // 'Start()' may throw, therefore store _factory for disposing.
    private readonly Lazy<IWebApplicationFactory> _factory;
    private readonly Lazy<StartedApplication> _app;
    private readonly OneTimeAppBuilder<TEntryPoint> _builder = new();

    public IApplicationConfigurator Configuration => _builder;

    public bool IsStarted => _app.IsValueCreated;

    /// <summary>
    /// Creates, memoizes and returns App.<br/>
    /// Access to <see cref="LazyApplication"/> finishes app building and starts App.
    /// </summary>
    public ITestApplication LazyApplication => _app.Value;

    public TestApplicationFixture()
    {
        // factory is created only when _app is creating
        _factory = new (_builder.Build, LazyThreadSafetyMode.None);
        _app = new (GetStartedApp);
    }
    
    private StartedApplication GetStartedApp()
    {
        // build
        var res = _factory.Value;

        //start
        res.StartServer();
        return new StartedApplication(res);
    }

    public ValueTask DisposeAsync()
    {
        if (_factory.IsValueCreated)
            return _factory.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}

internal class StartedApplication : ITestApplication
{
    private readonly IWebApplicationFactory _factory;

    public IServiceProvider Services => _factory.Services;

    public StartedApplication(IWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public HttpClient CreateClient() => _factory.CreateClient();
}

internal class OneTimeAppBuilder<TEntryPoint>: IApplicationConfigurator
where TEntryPoint: class
{
    private readonly TestApplicationBuilder<TEntryPoint> _builder = new();
    private IWebApplicationFactory? _instance;
    internal bool IsBuilt => _instance != null;

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        if(IsBuilt)
            throw new InvalidOperationException($"Can't use '{nameof(IApplicationConfigurator)}' after application is created.");

        _builder.ConfigureWebHost(action);
    }

    public IWebApplicationFactory Build()
    {
        _instance ??= _builder.Build();
        return _instance;
    }
}