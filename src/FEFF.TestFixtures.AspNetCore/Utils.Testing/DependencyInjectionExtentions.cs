using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtentions
{
    public static IServiceCollection TryReplaceSingleton<TService>(this IServiceCollection services, TService instance)
        where TService : class
    {
        var srcType = typeof(TService);
        var oldD = services.SingleOrDefault(d => d.ServiceType == srcType);
        if (oldD == null)
            return services;

        ThrowHelper.Assert(oldD.Lifetime == ServiceLifetime.Singleton);

        var sdNew = new ServiceDescriptor(srcType, instance);
        services.Replace(sdNew);

        return services;
    }
}