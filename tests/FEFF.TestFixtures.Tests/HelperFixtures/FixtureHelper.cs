namespace FEFF.TestFixtures.Tests;
using Core;

[Fixture]
internal class FixtureHelper : IAsyncDisposable
{
    public FixtureScopeManager FixtureManager { get; }
    public FixtureScope Scope { get; }

    private readonly Dictionary<string, string?> _additionalConfiguration = new();

    public FixtureHelper()
    {
        FixtureManager = new FixtureScopeManager((services) =>
            services.AddInMemoryAddConfiguration(_additionalConfiguration)
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
    /// Seactions are separated by '__' - double inderscore.
    /// </summary>
    public void UseSettingEnv(string name, string? value)
    {
        var n = name.Replace("__", ":");
        UseSetting(n, value);
    }

    /// <summary>
    /// Add Settings to configuration in default format:<br/>
    /// Seactions are separated by ':' - colon.
    /// </summary>
    public void UseSetting(string name, string? value)
    {
        _additionalConfiguration[name] = value;
    }
}