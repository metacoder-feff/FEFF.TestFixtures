using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Core;

internal static class FixtureCollector
{
    internal static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();

        AddBaseServices(services);

        var types = FindFixtureTypes<FixtureAttribute>();
        foreach (var t in types)
            RegisterFixtureType(services, t);
        
        return services;
    }

    private static void AddBaseServices(ServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            // .SetBasePath(Directory.GetCurrentDirectory())
            // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
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
//TODO: external scan
        TryInvokeRegisterExtended(t, services);

        services.AddScoped(t);
        var attribute = t.GetCustomAttribute<FixtureAttribute>();
        if (attribute?.RegisterWithType is null)
            return;
//TODO: analizer
        if(attribute.RegisterWithType.IsAssignableFrom(t) == false)
            throw new InvalidOperationException($"The implementation type'{t}' should be a subtype or implement {nameof(FixtureAttribute.RegisterWithType)} '{attribute.RegisterWithType}'.");

        services.AddScoped(attribute.RegisterWithType, sp => sp.GetRequiredService(t));
    }

    private static void TryInvokeRegisterExtended(Type t, ServiceCollection services)
    {
        if(t.GetInterfaces().Contains(typeof(IFixureRegistrator)) == false)
            return;

        var mi = typeof(FixtureCollector).GetMethod(nameof(RegisterExtended));
        var tt = typeof(FixtureCollector).GetMethods();

        var method = ThrowHelper.Guard.NotNull(
            typeof(FixtureCollector).GetMethod(nameof(RegisterExtended), BindingFlags.NonPublic | BindingFlags.Static)
        );

        method
            .MakeGenericMethod(t)
            .Invoke(null, [services])
            ;
    }

    private static void RegisterExtended<T>(ServiceCollection services)
    where T: IFixureRegistrator
    {
        T.RegisterFixture(services);
    }
}
