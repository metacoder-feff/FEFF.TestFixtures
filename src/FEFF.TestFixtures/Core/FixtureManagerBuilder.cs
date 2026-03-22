using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

public class FixtureManagerBuilder
{
    private readonly List<Action<IServiceCollection>> _actions = [];

    /// <remarks>
    /// Mandatory delegate with default implementation.
    /// </remarks>
    public Action<IServiceCollection> DiscoverFixturesAction { get; set; } = (services) => services.AddFixturesByReflection();

    /// <remarks>
    /// Add one or more optional delegates.
    /// </remarks>
    public void ConfigureServices(Action<IServiceCollection> action)
    {
        _actions.Add(action);
    }

    public FixtureManager Build()
    {
        var sp = BuildServiceProvider();
        var provider = new FixtureServiceProvider(sp);

        return new(provider);
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection()
            .AddConfiguration()
            .AddEnvironmentConfiguration()
            .Apply(DiscoverFixturesAction)
            ;

        foreach (var action in _actions)
            services.Apply(action);
        //== action(services);

        var sc = services.BuildServiceProvider(true);
        return sc;
    }
}