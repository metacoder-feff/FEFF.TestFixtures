using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Engine;

// Lazy<> is thread-safe by default
internal static class ReflectiveFixtureCollector
{
// TODO: cache callbacks only
    private static readonly Lazy<ServiceCollection> __cachedFixtureServices = new(CreateServiceCollection);
    
    private static readonly Lazy<MethodInfo> RegisterExtendedMethodInfo = new(() =>
        ThrowHelper.EnsureNotNull(
            typeof(ReflectiveFixtureCollector)
            .GetMethod(nameof(RegisterExtended), BindingFlags.NonPublic | BindingFlags.Static)
        )
    );

    private static ServiceCollection CreateServiceCollection()
    {
        var r = CreateServiceCollectionInternal();
        r.MakeReadOnly();
        return r;
    }

    internal static IServiceCollection AddFixturesByReflection(this IServiceCollection services)
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

        var aa = AssemblyDiscoveryService.GetAssemblies();
        foreach(var a in aa)
        {
            foreach(var t in a.GetTypes())
            {
                services.TryAddFixture(t);
                services.TryRegisterExtended(t);
            }
        }
        
        return services;
    }

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

//TODO: add analyzer
        // better to throw InvalidCastException when trying to resolve 'RegisterWithType'
        // if(attribute.RegisterWithType.IsAssignableFrom(t) == false)
        //     throw new InvalidOperationException($"The implementation type'{t}' should be a subtype or implement {nameof(FixtureAttribute.RegisterWithType)} '{attribute.RegisterWithType}'.");

        services.AddScoped(attribute.RegisterWithType, sp => sp.GetRequiredService(t));
    }

    private static void TryRegisterExtended(this ServiceCollection services, Type t)
    {
        if(t.GetInterfaces().Contains(typeof(IFixtureRegistrar)) == false)
            return;

        RegisterExtendedMethodInfo.Value
            .MakeGenericMethod(t)
            .Invoke(null, [services])
            ;
    }

    // Since an interface implementation may use a different method name or visibility modifier,
    // it is safer to invoke it through static linking rather than directly via reflection.
    private static void RegisterExtended<T>(ServiceCollection services)
    where T: IFixtureRegistrar
    {
        T.RegisterFixture(services);
    }
}