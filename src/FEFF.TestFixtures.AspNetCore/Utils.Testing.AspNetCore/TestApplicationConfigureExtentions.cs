namespace FEFF.Extentions.Testing.AspNetCore;

public enum AspEnvironment { Development, Production };

public static class TestApplicationConfigureExtentions
{
    public static void UseSetting(this IApplicationConfigurator builder, string key, string? value)
    {
        builder.ConfigureWebHost(
            b => b.UseSetting(key, value)
        );
    }

    public static void UseAspEnvironment(this IApplicationConfigurator builder, AspEnvironment env)
    {
        builder.ConfigureWebHost(
            b => b.UseEnvironment(env.ToString())
        );
    }

    public static void ConfigureServices(this IApplicationConfigurator builder, Action<IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void ConfigureServices(this IApplicationConfigurator builder, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }
}