using Microsoft.Extensions.Time.Testing;
using FEFF.Extentions.Testing.AspNetCore;
using Microsoft.Extensions.Logging.Testing;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Replaces <see cref="TimeProvider"/> service with <see cref="FakeTimeProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public sealed class FakeLoggerFixture : IDisposable
{
    private readonly FakeLoggerProvider _provider = new();

    public FakeLogCollector Collector => _provider.Collector;

    public FakeLoggerFixture(TestApplicationExtention app)
    {
        app.ConfigureServices(ReconfigureFactory);
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    private void ReconfigureFactory(IServiceCollection services)
    {
        services.AddLogging(b => b
            .ClearProviders()
            .AddProvider(_provider)
        );
    }
}