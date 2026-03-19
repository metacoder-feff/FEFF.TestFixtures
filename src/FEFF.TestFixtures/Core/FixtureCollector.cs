using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

internal static class FixtureCollector
{
    // thread-safe by default
    // TODO: cache callbacks only
    private static readonly Lazy<ServiceCollection> __cachedFixtureServices = new(CreateServiceCollection);

    private static ServiceCollection CreateServiceCollection()
    {
        var r = CreateServiceCollectionInternal();
        r.MakeReadOnly();
        return r;
    }

    internal static IServiceCollection AddFixtures(this IServiceCollection services)
    {
        foreach(var s in __cachedFixtureServices.Value)
        {
            services.Add(s);
        }
        return services;
    }

    /// <remarks>
    /// Heavy operation. Better to memoize the result.
    /// </remarks>
    private static ServiceCollection CreateServiceCollectionInternal()
    {
        var services = new ServiceCollection();

        foreach(var t in GetAllLoadedTypes())
        {
            services.TryAddFixture(t);
            services.TryRegisterExtended(t);
        }
        
        return services;
    }

//TODO: optimize?  
    private static IEnumerable<Type> GetAllLoadedTypes() =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            ;

    private static void TryAddFixture(this ServiceCollection services, Type t)
    {
        var attribute = t.GetCustomAttribute<FixtureAttribute>();
        if(attribute == null)
            return;

        // register primary type
        services.AddScoped(t);

        // register by supertype
        if (attribute.RegisterWithType is null)
            return;

//TODO: add analizer
        // better to throw InvalidCastException when trying to resolve 'RegisterWithType'
        // if(attribute.RegisterWithType.IsAssignableFrom(t) == false)
        //     throw new InvalidOperationException($"The implementation type'{t}' should be a subtype or implement {nameof(FixtureAttribute.RegisterWithType)} '{attribute.RegisterWithType}'.");

        services.AddScoped(attribute.RegisterWithType, sp => sp.GetRequiredService(t));
    }

    private static void TryRegisterExtended(this ServiceCollection services, Type t)
    {
        if(t.GetInterfaces().Contains(typeof(IFixureRegistrator)) == false)
            return;

//TODO: memoize?
        var method = ThrowHelper.Guard.NotNull(
            typeof(FixtureCollector).GetMethod(nameof(RegisterExtended), BindingFlags.NonPublic | BindingFlags.Static)
        );

        method
            .MakeGenericMethod(t)
            .Invoke(null, [services])
            ;
    }

    // Since an interface implementation may have a different method name, 
    // it's safer to call it through static linking rather than directly via reflection.
    private static void RegisterExtended<T>(ServiceCollection services)
    where T: IFixureRegistrator
    {
        T.RegisterFixture(services);
    }
}
