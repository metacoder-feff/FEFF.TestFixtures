using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures;

public interface IFixtureProvider
{
    T GetFixture<T>() where T : notnull;
}

public sealed class FixtureContainer : IAsyncDisposable, IFixtureProvider
{
    // thread-safe by default
    private static readonly Lazy<ServiceCollection> __services = new(CreateServiceCollection);
    private readonly ServiceProvider _provider;

    public FixtureContainer()
    {
        _provider = __services.Value.BuildServiceProvider(true);
    }

    private static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();

        var types = FindFixtureTypes<FixtureAttribute>();
        foreach (var t in types)
            RegisterFixureType(services, t);
        
        return services;
    }

    private static void RegisterFixureType(ServiceCollection services, Type t)
    {
        services.AddSingleton(t);
        var attribute = t.GetCustomAttribute<FixtureAttribute>();
        if (attribute?.RegisterWithType is null)
            return;
//TODO: analizer
        if(attribute.RegisterWithType.IsAssignableFrom(t) == false)
            throw new InvalidOperationException($"The implementation type'{t}' should be a subtype or implement {nameof(FixtureAttribute.RegisterWithType)} '{attribute.RegisterWithType}'.");

        services.AddSingleton(attribute.RegisterWithType, sp => sp.GetRequiredService(t));
    }

    private static IEnumerable<Type> FindFixtureTypes<TAttribute>()
    {
        var atr = typeof(TAttribute);
        
        var types = GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsDefined(atr, false));
        return types;
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
//TODO: optimize?        
        // var assembly = Assembly.GetExecutingAssembly();
        // return [assembly];
        
        return AppDomain.CurrentDomain.GetAssemblies();
    }

    public ValueTask DisposeAsync()
    {
        return _provider.DisposeAsync();
    }

    public T GetFixture<T>()
    where T : notnull
    {
        return _provider.GetRequiredService<T>();
    }
}
