using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

/// <summary>
/// This class creates <see cref="FixtureScope"/>.<br/>
/// User must call <see cref="FixtureScope.DisposeAsync"/> for each created <see cref="FixtureScope"/>.
/// </summary>
public sealed class FixtureScopeFactory : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public FixtureScopeFactory(IServiceCollection services)
    {
        _provider = services.BuildServiceProvider(true);
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
