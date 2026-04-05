using System.Data.Common;

namespace FEFF.TestFixtures.AspNetCore;

public enum AspEnvironment { Development, Production };

public static class TestApplicationConfigureExtensions
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

    public static void UseTimeProvider(this IApplicationConfigurator builder, TimeProvider timeProvider)
    {
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<TimeProvider>(timeProvider)
        );
    }

    public static void UseRandom(this IApplicationConfigurator builder, Random random)
    {
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<Random>(random)
        );
    }

    public static void UseLoggerProvider(this IApplicationConfigurator builder, ILoggerProvider loggerProvider)
    {
        builder.ConfigureServices(services =>
            services.AddLogging(b => b
                // .ClearProviders()
                .AddProvider(loggerProvider)
            )
        );
    }

    public static void UseTmpDatabaseName(this IApplicationConfigurator builder, TmpScopeIdFixture tmpScopeIdfx, string connectionStringName, params string[] additionalConnectionStringNames)
    {
        builder.UseDatabaseNamePostfix($"test-{tmpScopeIdfx.Value}", connectionStringName, additionalConnectionStringNames);
    }

    public static void UseDatabaseNamePostfix(this IApplicationConfigurator builder, string suffix, string connectionStringName, params string[] additionalConnectionStringNames)
    {
        builder.ConfigureServices((ctx, _) =>
        {
            var sss = additionalConnectionStringNames.Append(connectionStringName);
            foreach(var csn in sss)
                ctx.Configuration.AddSuffixToConnectionString(csn, suffix);
        });
    }
    
    private static void AddSuffixToConnectionString(this IConfiguration config, string connectionStringName, string suffix)
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