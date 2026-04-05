using Microsoft.Extensions.Logging.Testing;

namespace FEFF.TestFixtures.AspNetCore;

[Fixture]
public class FakeLoggerFixture : IDisposable, ITestApplicationExtension
{
    protected readonly FakeLoggerProvider _provider = new();

    public FakeLogCollector Collector => _provider.Collector;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _provider.Dispose();
    }

    public void Configure(ITestApplicationFixture app)
    {
        app.Configuration.ConfigureServices(ReconfigureFactory);
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.AddLogging(b => b
            // .ClearProviders()
            .AddProvider(_provider)
        );
    }
}

/// <summary>
/// Replaces the <see cref="ILoggerProvider"/> service with a <see cref="FakeLoggerProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeLoggerFixture<TEntryPoint> : FakeLoggerFixture
where TEntryPoint: class
{
    public FakeLoggerFixture(TestApplicationFixture<TEntryPoint> app)
    {
        Configure(app);
    }
}