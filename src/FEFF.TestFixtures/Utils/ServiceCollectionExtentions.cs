//TODO: nuget

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtentions
{
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

        services.AddSingleton<IConfigurationRoot>((sp) => sp
            .GetRequiredService<IOptions<ConfigurationBuilder>>()
            .Value
            .Build()
        );

        services.AddSingleton<IConfiguration>((sp) => sp
            .GetRequiredService<IConfigurationRoot>()
        );

        return services;
    }

    internal static IServiceCollection AddEnvironmentConfiguration(this IServiceCollection services)
    {
        services.Configure<ConfigurationBuilder>(b => b
            .AddEnvironmentVariables()
        );

        return services;
    }

    internal static IServiceCollection AddInMemoryConfiguration(this IServiceCollection services, Dictionary<string, string?>? additional)
    {
        services.Configure<ConfigurationBuilder>(b => b
            .AddInMemoryCollection(additional)
        );

        return services;
    }
}
