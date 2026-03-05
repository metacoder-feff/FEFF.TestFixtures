namespace FEFF.TestFixtures.Tests;

public class XunitIntegratedFixtureTestBase
{
    //----------------------------------------------
    /// <summary>
    /// Get a TestCase-Fixture. It would be destroyed after TestCase finishes.
    /// </summary>
    /// <remarks>
    /// TestCase-Fixure integration requires:
    /// <code>
    /// [assembly: FEFF.Experimental.TestFixtures.FixturesXUnitExtension]
    /// </code>
    /// Otherwise manage <see cref="IFixtureProvider"/> manually.
    /// </remarks>
    protected static T GetFixture<T>()
    where T : notnull
    {
        return TestContext.Current.GetTestCaseFixtureProvider().GetFixture<T>();
    }
}