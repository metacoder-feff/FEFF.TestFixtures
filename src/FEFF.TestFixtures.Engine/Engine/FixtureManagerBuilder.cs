using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Engine;

/// <summary>
/// Configuration options for building a <see cref="FixtureManager"/>.
/// </summary>
public class FixtureManagerOptions : IFixtureManagerOptions
{
    private readonly List<Action<IServiceCollection>> _actions = [];

    /// <summary>
    /// Gets or sets the delegate responsible for discovering fixtures.
    /// Defaults to scanning assemblies via reflection for types marked with <see cref="FixtureAttribute"/> or implementing <see cref="IFixtureRegistrar"/>.
    /// </summary>
    /// <remarks>
    /// Mandatory delegate with default implementation.
    /// </remarks>
    public Action<IServiceCollection> DiscoverFixturesAction { get; set; } = (services) => services.AddFixturesByReflection();

    /// <summary>
    /// Adds an optional action to further configure the service collection.
    /// </summary>
    /// <param name="action">The configuration action to add.</param>
    /// <remarks>
    /// Add one or more optional delegates.
    /// </remarks>
    public void ConfigureServices(Action<IServiceCollection> action)
    {
        _actions.Add(action);
    }

    /// <summary>
    /// Builds the <see cref="ServiceProvider"/> with all configured fixtures and services.
    /// </summary>
    /// <returns>The built <see cref="ServiceProvider"/>.</returns>
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

/// <summary>
/// Builder for creating a <see cref="FixtureManager"/> instance.
/// </summary>
public class FixtureManagerBuilder
{
    /// <summary>
    /// Gets the options used to configure the <see cref="FixtureManager"/>.
    /// </summary>
    public FixtureManagerOptions Options { get; set; } = new();

    /// <summary>
    /// Builds and returns a new <see cref="FixtureManager"/> instance.
    /// </summary>
    /// <returns>A new <see cref="FixtureManager"/> instance.</returns>
    public FixtureManager Build()
    {
        return new(Options);
    }
}
