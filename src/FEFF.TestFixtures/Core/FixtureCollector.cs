using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FEFF.TestFixtures.Core;

internal static class FixtureCollector
{
    /// <remarks>
    /// Heavy operation. Better to memoize the result.
    /// </remarks>
    internal static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();

        foreach(var t in GetAllLoadedTypes())
        {
            services.TryAddFixture(t);
            services.TryRegisterExtended(t);
        }
        
        return services;
    }

    internal static IServiceCollection Apply(this IServiceCollection services, Action<IServiceCollection>? action)
    {
        action?.Invoke(services);
        return services;
    }

    internal static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        // .SetBasePath(Directory.GetCurrentDirectory())
        // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //     ;

        services.Configure<ConfigurationBuilder>(b => b
            .AddEnvironmentVariables()
        );

        services.AddSingleton<IConfiguration>((sp) => sp
            .GetRequiredService<IOptions<ConfigurationBuilder>>()
            .Value
            .Build()
        );

        return services;
    }

    internal static IServiceCollection AddInMemoryAddConfiguration(this IServiceCollection services, Dictionary<string, string?>? additional)
    {
        services.Configure<ConfigurationBuilder>(b => b
            .AddInMemoryCollection(additional)
        );

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
