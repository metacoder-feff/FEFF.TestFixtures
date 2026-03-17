using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

/// <summary>
/// This class creates <see cref="FixtureScope"/>.<br/>
/// User must call <see cref="FixtureScope.DisposeAsync"/> for each created <see cref="FixtureScope"/>.
/// </summary>
public sealed class FixtureScopeFactory : IAsyncDisposable
{
    // thread-safe by default
    private static readonly Lazy<ServiceCollection> __services = new(FixtureCollector.CreateServiceCollection);

    private readonly ServiceProvider _provider;

    public FixtureScopeFactory()
    {
        _provider = __services.Value.BuildServiceProvider(true);
    }

    public ValueTask DisposeAsync()
    {
        return _provider.DisposeAsync();
    }

    public FixtureScope CreateScope()
    {
        return new FixtureScope(_provider);
    }
}
