using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

internal static class FixtureCollector
{
    internal static ServiceCollection CollectFixtureTypes()
    {
        var services = new ServiceCollection();

        var types = FindFixtureTypes<FixtureAttribute>();
        foreach (var t in types)
            RegisterFixtureType(services, t);
        
        return services;
    }

    private static IEnumerable<Type> FindFixtureTypes<TAttribute>()
    {
        var atr = typeof(TAttribute);
        
        return GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsDefined(atr, false));
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
//TODO: optimize?        
        // var assembly = Assembly.GetExecutingAssembly();
        // return [assembly];
        
        return AppDomain.CurrentDomain.GetAssemblies();
    }

    private static void RegisterFixtureType(ServiceCollection services, Type t)
    {
        services.AddScoped(t);
        var attribute = t.GetCustomAttribute<FixtureAttribute>();
        if (attribute?.RegisterWithType is null)
            return;
//TODO: analizer
        if(attribute.RegisterWithType.IsAssignableFrom(t) == false)
            throw new InvalidOperationException($"The implementation type'{t}' should be a subtype or implement {nameof(FixtureAttribute.RegisterWithType)} '{attribute.RegisterWithType}'.");

        services.AddScoped(attribute.RegisterWithType, sp => sp.GetRequiredService(t));
    }
}
