namespace FEFF.TestFixtures.Tests;
using Core;

/// <summary>
/// A base class for tests with basic XUnit Test Teardown via IAsyncDisposable
/// </summary>
/// <remarks>
/// Do not use fixtures to test fixtures here
/// <remarks/>
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

    public FixtureTestBase()
    {
        FixtureManager = new FixtureScopeManager();
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
}