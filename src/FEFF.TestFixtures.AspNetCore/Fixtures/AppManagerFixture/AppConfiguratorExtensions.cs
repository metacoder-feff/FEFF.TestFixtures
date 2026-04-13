using System.Data.Common;

namespace FEFF.TestFixtures.AspNetCore;

/// <summary>
/// Specifies the ASP.NET Core environment for the test application.
/// </summary>
public enum AspEnvironment
{
    /// <summary>
    /// Development environment. Enables developer exception pages and verbose logging.
    /// </summary>
    Development,

    /// <summary>
    /// Production environment. Optimized for performance with minimal diagnostic output.
    /// </summary>
    Production
}

/// <summary>
/// Extension methods for <see cref="IAppConfigurator"/> to simplify test application configuration.
/// </summary>
public static class AppConfiguratorExtensions
{
    /// <summary>
    /// Sets a configuration setting on the web host.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    public static void UseSetting(this IAppConfigurator builder, string key, string? value)
    {
        builder.ConfigureWebHost(
            b => b.UseSetting(key, value)
        );
    }

    /// <summary>
    /// Sets the ASP.NET Core environment for the web host.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="env">The environment to use.</param>
    public static void UseAspEnvironment(this IAppConfigurator builder, AspEnvironment env)
    {
        builder.ConfigureWebHost(
            b => b.UseEnvironment(env.ToString())
        );
    }

    /// <summary>
    /// Adds a delegate for configuring the <see cref="IServiceCollection"/> on the web host.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="configureServices">A delegate for configuring services.</param>
    public static void ConfigureServices(this IAppConfigurator builder, Action<IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    /// <summary>
    /// Adds a delegate for configuring the <see cref="IServiceCollection"/> on the web host
    /// with access to the <see cref="WebHostBuilderContext"/>.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="configureServices">A delegate for configuring services.</param>
    public static void ConfigureServices(this IAppConfigurator builder, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    /// <summary>
    /// Replaces the application's <see cref="TimeProvider"/> with a custom instance.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="timeProvider">The <see cref="TimeProvider"/> instance to use.</param>
    public static void UseTimeProvider(this IAppConfigurator builder, TimeProvider timeProvider)
    {
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<TimeProvider>(timeProvider)
        );
    }

    /// <summary>
    /// Replaces the application's <see cref="Random"/> with a custom instance.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="random">The <see cref="Random"/> instance to use.</param>
    public static void UseRandom(this IAppConfigurator builder, Random random)
    {
        builder.ConfigureServices(services =>
            services.TryReplaceSingleton<Random>(random)
        );
    }

    /// <summary>
    /// Adds a custom <see cref="ILoggerProvider"/> to the application's logging pipeline.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="loggerProvider">The logger provider to add.</param>
    public static void UseLoggerProvider(this IAppConfigurator builder, ILoggerProvider loggerProvider)
    {
        builder.ConfigureServices(services =>
            services.AddLogging(b => b
                // .ClearProviders()
                .AddProvider(loggerProvider)
            )
        );
    }

    /// <summary>
    /// Appends a suffix to the database name in the specified connection strings.
    /// Useful for creating isolated databases per test.
    /// </summary>
    /// <param name="builder">The app configurator.</param>
    /// <param name="suffix">The suffix to append to the database name.</param>
    /// <param name="connectionStringNames">The names of the connection strings to modify.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionStringNames"/> is null or empty.</exception>
    public static void UseDatabaseNamePostfix(this IAppConfigurator builder, string suffix, IReadOnlyCollection<string> connectionStringNames)
    {
        ThrowHelper.Argument.ThrowIfNullOrEmpty(connectionStringNames);
        builder.ConfigureServices((ctx, _) =>
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
