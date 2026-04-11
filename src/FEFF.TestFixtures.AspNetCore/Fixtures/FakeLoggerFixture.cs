using Microsoft.Extensions.Logging.Testing;

namespace FEFF.TestFixtures.AspNetCore;

[Fixture]
public class FakeLoggerFixture : IDisposable
{
    private readonly FakeLoggerProvider _provider = new();

    public FakeLogCollector Collector => _provider.Collector;
    public ILoggerProvider LoggerProvider => _provider;

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
}

/// <summary>
/// Replaces the <see cref="ILoggerProvider"/> service with a <see cref="FakeLoggerProvider"/> singleton in a tested application.
/// </summary>
[Fixture]
public class FakeLoggerFixture<TEntryPoint> : FakeLoggerFixture
where TEntryPoint: class
{
    public FakeLoggerFixture(AppManagerFixture<TEntryPoint> app)
    {
        app.ConfigurationBuilder.UseLoggerProvider(LoggerProvider);
    }
}