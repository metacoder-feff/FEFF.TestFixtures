using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Tests;
using Core;

[Fixture]
internal class FixtureHelper : IAsyncDisposable
{
    public const string ScopeId = "testing-scope-1";
    public FixtureManager FixtureManager { get; }
    public IFixtureScope Scope { get; }

    private readonly Dictionary<string, string?> _additionalConfiguration = [];

    public FixtureHelper()
    {
        var builder = new FixtureManagerBuilder();
        builder.Options.ConfigureServices((services) =>
            services.AddInMemoryConfiguration(_additionalConfiguration)
        );

        FixtureManager = builder.Build();
        Scope = FixtureManager.GetScope(ScopeId);
    }

    public ValueTask DisposeAsync()
    {
        return FixtureManager.DisposeAsync();
    }

    public T GetFixture<T>()
    where T : notnull
    {
        return Scope.GetFixture<T>();
    }

    /// <summary>
    /// Add Settings to configuration in 'env' format:<br/>
    /// Sections are separated by '__' - double inderscore.
    /// </summary>
    public void UseSettingEnv(string name, string? value)
    {
        var n = name.Replace("__", ":");
        UseSetting(n, value);
    }

    /// <summary>
    /// Add Settings to configuration in default format:<br/>
    /// Sections are separated by ':' - colon.
    /// </summary>
    public void UseSetting(string name, string? value)
    {
        _additionalConfiguration[name] = value;
    }
}