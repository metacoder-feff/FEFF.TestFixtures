using FEFF.Extentions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

//TODO: doc TEntryPoint
//TODO: async StartServerAsync-> OnStartedHandlerAsync[]
//  e.g. DB.Create

[Fixture]
public class TestApplicationExtention : IApplicationConfigurator
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];

    public IReadOnlyList<Action<IWebHostBuilder>> Actions => _builderOverrides.AsReadOnly();

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        _builderOverrides.Add(action);
    }
}

public interface ITestApplicationFixture
{
    IApplicationConfigurator Configuration   { get; }
    ITestApplication         LazyApplication { get; }
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
    private readonly TestApplicationExtention _ext;

    public IApplicationConfigurator Configuration
    {
        get
        {
            if(_app.IsValueCreated)
                throw new InvalidOperationException("Can't use ApplicationBuilder after application is created.");

            return _ext;
        }
    }

    /// <summary>
    /// Creates, memoizes and returns App. The App may be started.<br/>
    /// Access to <see cref="LazyApplication"/> finishes app building.
    /// </summary>
    public ITestApplication LazyApplication => _app.Value;

    public TestApplicationFixture(TestApplicationExtention ext)
    {
        _app = new (Create);
        _ext = ext;
    }

    private ITestApplication Create()
    {
        return TestApplicationBuilder.Build<TEntryPoint>(_ext.Actions);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app.IsValueCreated)
            await _app.Value.DisposeAsync().ConfigureAwait(false);
    }
}
