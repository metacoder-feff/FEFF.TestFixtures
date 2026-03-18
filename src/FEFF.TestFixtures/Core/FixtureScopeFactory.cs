using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

/// <summary>
/// This class creates <see cref="FixtureScope"/>.<br/>
/// User must call <see cref="FixtureScope.DisposeAsync"/> for each created <see cref="FixtureScope"/>.
/// </summary>
public sealed class FixtureScopeFactory : IAsyncDisposable
{
    // thread-safe by default
    // TODO: cache callbacks only
    private static readonly Lazy<ServiceCollection> __cachedFixtureServices = new(CreateServiceCollection);

    private static ServiceCollection CreateServiceCollection()
    {
        var r = FixtureCollector.CreateServiceCollection();
        r.MakeReadOnly();
        return r;
    }

    private readonly ServiceProvider _provider;

    public FixtureScopeFactory(Dictionary<string, string?>? additionalConfiguration = null)
    {
        _provider = Clone(__cachedFixtureServices.Value)
            .AddConfiguration(additionalConfiguration)
            .BuildServiceProvider(true)
            ;
    }

    private static IServiceCollection Clone(IServiceCollection src)
    {
        IServiceCollection res = new ServiceCollection();
        foreach (var service in src)
        {
            res.Add(service);
        }
        return res;
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
