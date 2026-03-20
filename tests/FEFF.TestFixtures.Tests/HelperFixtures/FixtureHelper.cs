using Microsoft.Extensions.DependencyInjection;

namespace FEFF.TestFixtures.Tests;
using Core;

[Fixture]
internal class FixtureHelper : IAsyncDisposable
{
    public FixtureManager FixtureManager { get; }
    public FixtureScope Scope { get; }

    private readonly Dictionary<string, string?> _additionalConfiguration = [];

    public FixtureHelper()
    {
        FixtureManager = new FixtureManager((services) =>
            services.AddInMemoryConfiguration(_additionalConfiguration)
        );
        Scope = FixtureManager.GetScope("testing-scope-1");
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