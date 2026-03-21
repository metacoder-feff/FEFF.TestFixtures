using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

public class FixtureServiceProviderBuilder : IFixtureServiceProviderBuilder
{
    private readonly List<Action<IServiceCollection>> _actions = [];

    public void Configure(Action<IServiceCollection> action)
    {
        _actions.Add(action);
    }

    public FixtureServiceProvider Build()
    {
        var services = new ServiceCollection()
            .AddConfiguration()
            .AddEnvironmentConfiguration()
            .AddFixturesByReflection() // TODO: optionally replace
            ;

        foreach(var action in _actions)
            action(services);
            //services.Apply(action);

        return new(services);
    }
}