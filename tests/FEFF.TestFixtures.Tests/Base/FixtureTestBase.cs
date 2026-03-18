namespace FEFF.TestFixtures.Tests;
using Core;

//TODO: use fuxture instead of base class
public class FixtureTestBase : IAsyncDisposable
{
    //----------------------------------------------
    /// <summary>
    /// Get a TestCase-Fixture. It would be destroyed after its scope (TestCase) finishes.
    /// </summary>
    protected T GetFixture<T>()
    where T : notnull
    {
        return Scope.GetFixture<T>();
    }

    protected FixtureScopeManager FixtureManager { get; }
    protected FixtureScope Scope { get; }

    private readonly Dictionary<string, string?> __additionalConfiguration = new();

    public FixtureTestBase()
    {
        FixtureManager = new FixtureScopeManager(__additionalConfiguration);
        Scope = FixtureManager.GetScope("testing-scope-1");
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);;
        GC.SuppressFinalize(this);
    }
    
    protected async virtual ValueTask DisposeAsyncCore()
    {
        await FixtureManager.DisposeAsync().ConfigureAwait(false);;
    }

    /// <summary>
    /// Add Settings to configuration in 'env' format:<br/>
    /// Seactions are separated by '__' - double inderscore.
    /// </summary>
    protected void UseSettingEnv(string name, string? value)
    {
        var n = name.Replace("__", ":");
        UseSetting(n, value);
    }

    /// <summary>
    /// Add Settings to configuration in default format:<br/>
    /// Seactions are separated by ':' - colon.
    /// </summary>
    protected void UseSetting(string name, string? value)
    {
        __additionalConfiguration[name] = value;
    }
}