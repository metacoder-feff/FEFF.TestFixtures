using Microsoft.Extensions.Logging.Testing;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Replaces the <see cref="ILoggerProvider"/> service with a <see cref="FakeLoggerProvider"/> 
/// singleton in the application under test.
/// Provides a <see cref="FakeLogCollector"/> for exploring captured application log output in tests.
/// </summary>
/// <typeparam name="TEntryPoint">The application entry point type.</typeparam>
[Fixture]
public class FakeLoggerFixture<TEntryPoint> : IDisposable
where TEntryPoint : class
{
    private readonly FakeLoggerProvider _provider = new();

    /// <summary>
    /// Gets the collector for retrieving captured log entries.
    /// </summary>
    public FakeLogCollector Collector => _provider.Collector;

    /// <summary>
    /// Gets the <see cref="FakeLoggerProvider"/> as an <see cref="ILoggerProvider"/>.
    /// </summary>
    public ILoggerProvider LoggerProvider => _provider;

    /// <summary>
    /// Creates a new <see cref="FakeLoggerFixture{TEntryPoint}"/> and registers the fake logger provider
    /// with the application under test.
    /// </summary>
    /// <param name="app">The application manager fixture.</param>
    public FakeLoggerFixture(AppManagerFixture<TEntryPoint> app)
    {
        app.ConfigurationBuilder.UseLoggerProvider(LoggerProvider);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    /// <summary>
    /// Releases the resources used by the fixture.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _provider.Dispose();
    }
}
