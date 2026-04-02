using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Engine;

public class FixtureManagerOptions : IFixtureManagerOptions
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

    public ServiceProvider BuildServiceProvider()
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

public class FixtureManagerBuilder
{
    public FixtureManagerOptions Options { get; set; } = new();

    public FixtureManager Build()
    {
        return new(Options);
    }
}