namespace FEFF.TestFixtures.Tests;
using Core;

/// <summary>
/// A base class for tests with basic XUnit Test Teardown via IAsyncDisposable
/// </summary>
public class FixtureScopeTestBase : IAsyncDisposable
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

    protected FixtureScopeFactory Factory;
    protected FixtureScope Scope;

    public FixtureScopeTestBase()
    {
        Factory = new FixtureScopeFactory();
        Scope = Factory.CreateScope();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);;
        GC.SuppressFinalize(this);
    }
    
    protected async virtual ValueTask DisposeAsyncCore()
    {
        await Scope.DisposeAsync().ConfigureAwait(false);;
        await Factory.DisposeAsync().ConfigureAwait(false);;
    }
}