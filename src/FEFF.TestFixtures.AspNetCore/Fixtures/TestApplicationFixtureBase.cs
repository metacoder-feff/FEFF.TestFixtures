using FEFF.Extentions.Testing.AspNetCore;

namespace FEFF.TestFixtures.AspNetCore;

public interface ITestApplicationFixture
{
    ITestApplicationBuilder ApplicationBuilder  { get; }
    ITestApplication        LazyCreatedApplication { get; }
}

//TODO: doc TEntryPoint
//TODO: async StartServerAsync-> OnStartedHandlerAsync[]
//  e.g. DB.Create


/// <summary>
/// Allows to configure and start tested application via <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{}"/>.<br/>
/// User should derive a fixture class with fixed <see cref="TEntryPoint"/> and add <see cref="FixtureAttribute(FixtureType = typeof(ITestApplicationFixture))"/> to register.
/// </summary>
/// <typeparam name="TEntryPoint"></typeparam>
public sealed class TestApplicationFixtureBase<TEntryPoint> : IAsyncDisposable, ITestApplicationFixture
where TEntryPoint: class
{
    private readonly TestApplicationBuilder<TEntryPoint> _appBuilder = new();
    private readonly Lazy<ITestApplication> _app;

    public ITestApplicationBuilder ApplicationBuilder
    {
        get
        {
            if(_app.IsValueCreated)
                throw new InvalidOperationException("Can't use ApplicationBuilder after application is created.");
            return _appBuilder;
        }
    }

    /// <summary>
    /// Creates, memoizes and returns App. The App may be started.<br/>
    /// Access to <see cref="LazyCreatedApplication"/> finishes app building.
    /// </summary>
    public ITestApplication LazyCreatedApplication => _app.Value;

    public TestApplicationFixtureBase()
    {
        _app = new (_appBuilder.Build);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app.IsValueCreated)
            await _app.Value.DisposeAsync().ConfigureAwait(false);
    }
}
