using FEFF.Extensions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

//TODO: doc TEntryPoint
//TODO: async StartServerAsync-> OnStartedHandlerAsync[]
//  e.g. DB.Create

public interface ITestApplicationFixture
{
    IApplicationConfigurator Configuration   { get; }
    ITestApplication         LazyApplication { get; }
}

public interface ITestApplicationExtension
{
    void Configure(ITestApplicationFixture app);
}

/// <summary>
/// Allows to configure and start tested application via <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{}"/>
/// </summary>
/// <typeparam name="TEntryPoint">
/// A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.
/// </typeparam>
[Fixture]
public sealed class TestApplicationFixture<TEntryPoint> : IAsyncDisposable, ITestApplicationFixture
where TEntryPoint: class
{
    private readonly Lazy<ITestApplication> _app;
    private readonly OneTimeAppBuilder<TEntryPoint> _builder = new();

    public IApplicationConfigurator Configuration => _builder;

    /// <summary>
    /// Creates, memoizes and returns App.<br/>
    /// Access to <see cref="LazyApplication"/> finishes app building.
    /// </summary>
    public ITestApplication LazyApplication => _app.Value;

    public TestApplicationFixture()
    {
        _app = new (_builder.Build);
    }

    public ValueTask DisposeAsync()
    {
        if (_app.IsValueCreated)
            return _app.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}

internal class OneTimeAppBuilder<TEntryPoint>: IApplicationConfigurator
where TEntryPoint: class
{
    private readonly TestApplicationBuilder<TEntryPoint> _builder = new();
    private ITestApplication? _instance;
    internal bool IsBuilt => _instance != null;

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        if(IsBuilt)
            throw new InvalidOperationException($"Can't use '{nameof(IApplicationConfigurator)}' after application is created.");

        _builder.ConfigureWebHost(action);
    }

    public ITestApplication Build()
    {
        _instance ??= _builder.Build();
        return _instance;
    }
}

public static class TestApplicationFixtureExt
{
    // Chain result configuration of 'ITestApplicationFixture'
    public static ITestApplicationFixture AttachExtensions(this ITestApplicationFixture src, params ITestApplicationExtension[] extensions)
    {
        foreach(var e in extensions)
            e.Configure(src);
        
        return src;
    }
}