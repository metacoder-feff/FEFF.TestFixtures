namespace FEFF.TestFixtures.Tests;

/// <summary>
/// A base class for tests with basic XUnit Test Teardown via IAsyncDisposable
/// </summary>
public class VanillaFixtureTestBase : IAsyncDisposable
{
    //----------------------------------------------
    /// <summary>
    /// Get a TestCase-Fixture. It would be destroyed after TestCase finishes.
    /// </summary>
    protected T GetFixture<T>()
    where T : notnull
    {
        return Container.GetFixture<T>();
    }

    protected FixtureContainer Container;

    public VanillaFixtureTestBase()
    {
        Container = new FixtureContainer();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return DisposeAsyncCore();
    }
    
    protected virtual ValueTask DisposeAsyncCore()
    {
        return Container.DisposeAsync();
    }
}