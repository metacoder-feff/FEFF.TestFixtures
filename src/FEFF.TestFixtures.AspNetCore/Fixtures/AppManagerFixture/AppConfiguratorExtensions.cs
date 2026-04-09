using System.Data.Common;

namespace FEFF.TestFixtures.AspNetCore;

public enum AspEnvironment { Development, Production };

public static class AppConfiguratorExtensions
{
    public static void UseSetting(this IAppConfigurator builder, string key, string? value)
    {
        builder.ConfigureWebHost(
            b => b.UseSetting(key, value)
        );
    }

    public static void UseAspEnvironment(this IAppConfigurator builder, AspEnvironment env)
    {
        builder.ConfigureWebHost(
            b => b.UseEnvironment(env.ToString())
        );
    }

    public static void ConfigureServices(this IAppConfigurator builder, Action<IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void ConfigureServices(this IAppConfigurator builder, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void UseTimeProvider(this IAppConfigurator builder, TimeProvider timeProvider)
    {
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<TimeProvider>(timeProvider)
        );
    }

    public static void UseRandom(this IAppConfigurator builder, Random random)
    {
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<Random>(random)
        );
    }

    public static void UseLoggerProvider(this IAppConfigurator builder, ILoggerProvider loggerProvider)
    {
        builder.ConfigureServices(services =>
            services.AddLogging(b => b
                // .ClearProviders()
                .AddProvider(loggerProvider)
            )
        );
    }
    
    public static void UseDatabaseNamePostfix(this IAppConfigurator builder, string suffix, IReadOnlyCollection<string> connectionStringNames)
    {
        ThrowHelper.Argument.ThrowIfNullOrEmpty(connectionStringNames);
        builder.ConfigureServices( (ctx, _) =>
        {
            foreach (string item in connectionStringNames)
            {
                ctx.Configuration.AddSuffixToConnectionString(item, suffix);
            }
        });
    }
    
    internal static void AddSuffixToConnectionString(this IConfiguration config, string connectionStringName, string suffix)
    {
        var key = "ConnectionStrings:" + connectionStringName;
        var cs = config[key];
        var csb = new DbConnectionStringBuilder
        {
            ConnectionString = cs
        };
        csb["Database"] = $"{csb["Database"]}-{suffix}";
        var newCs = csb.ConnectionString;
        config[key] = newCs;

        // _oldCs = cs;
        // _newCs = newCs;
    }
}