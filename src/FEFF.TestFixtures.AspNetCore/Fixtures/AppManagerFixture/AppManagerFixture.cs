using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Represents a test application running via <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public interface ITestApplication
{
    /// <summary>
    /// Gets the service provider for the test application.
    /// </summary>
    IServiceProvider Services { get; }
    /// <summary>
    /// Gets the <see cref="TestServer"/> for the test application. Useful for SignalR testing.
    /// </summary>
    TestServer Server { get; } // for signal-r

    /// <summary>
    /// Creates an <see cref="HttpClient"/> configured to communicate with the test application.
    /// </summary>
    HttpClient CreateClient();
}

/// <summary>
/// Provides a way to configure the test application's web host before it starts.
/// </summary>
public interface IAppConfigurator
{
    /// <summary>
    /// Adds a configuration action to be applied to the <see cref="IWebHostBuilder"/>.
    /// </summary>
    /// <param name="action">The configuration action.</param>
    void ConfigureWebHost(Action<IWebHostBuilder> action);
}

/// <summary>
/// Provides access to the application configuration and lifecycle state.
/// </summary>
public interface IAppManagerFixture
{
    /// <summary>
    /// Gets the configurator used to customize the web host before startup.
    /// </summary>
    IAppConfigurator ConfigurationBuilder { get; }

    /// <summary>
    /// Gets the lazily-started test application.
    /// </summary>
    /// <remarks>
    /// Starts the application under test on first access.
    /// </remarks>
    ITestApplication LazyApplication { get; }

    /// <summary>
    /// Gets a value indicating whether the application has been started.
    /// </summary>
    bool IsStarted { get; }
}

/// <summary>
/// Allows configuring and starting the tested application via <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
/// <typeparam name="TEntryPoint">
/// A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.
/// </typeparam>
[Fixture]
public sealed class AppManagerFixture<TEntryPoint> : IAsyncDisposable, IAppManagerFixture
where TEntryPoint : class
{
    // 'Start()' may throw, therefore store _factory for disposing.
    private readonly Lazy<IWebApplicationFactory> _factory;
    private readonly Lazy<ITestApplication> _app;
    private readonly OneTimeAppBuilder<TEntryPoint> _builder = new();

    /// <inheritdoc/>
    public IAppConfigurator ConfigurationBuilder => _builder;
    private bool _isDisposed;

    /// <inheritdoc/>
    public bool IsStarted => _app.IsValueCreated;

    /// <inheritdoc/>
    public ITestApplication LazyApplication => _app.Value;

    /// <summary>
    /// Initializes a new instance of <see cref="AppManagerFixture{TEntryPoint}"/>.
    /// </summary>
    public AppManagerFixture()
    {
        // factory is created only when _app is creating
        _factory = new(_builder.Build, LazyThreadSafetyMode.None);
        _app = new(GetStartedApp);
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

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _isDisposed = true;

        if (_factory.IsValueCreated)
            return _factory.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}

internal class OneTimeAppBuilder<TEntryPoint> : IAppConfigurator
where TEntryPoint : class
{
    private readonly ApplicationBuilder<TEntryPoint> _builder = new();
    private IWebApplicationFactory? _instance;
    internal bool IsBuilt => _instance != null;

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        if (IsBuilt)
            throw new InvalidOperationException($"Can't use '{nameof(IAppConfigurator)}' after application is created.");

        _builder.ConfigureWebHost(action);
    }

    public IWebApplicationFactory Build()
    {
        _instance ??= _builder.Build();
        return _instance;
    }
}
