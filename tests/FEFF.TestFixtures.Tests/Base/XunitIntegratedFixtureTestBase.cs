using FEFF.TestFixtures.Xunit;

// register the extension
[assembly: TestFixturesExtension]

namespace FEFF.TestFixtures.Tests;

public class XunitIntegratedFixtureTestBase
{
    //----------------------------------------------
    /// <summary>
    /// Get a TestCase-Fixture. It would be destroyed after its scope finishes.
    /// </summary>
    /// <remarks>
    /// TestCase-Fixture integration requires:
    /// <code>
    /// [assembly: TestFixturesExtension]
    /// </code>
    /// Otherwise manage <see cref="IFixtureProvider"/> manually.
    /// </remarks>
    protected static T GetFixture<T>(FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        return TestContext.Current.GetFeffFixture<T>(scopeType);
    }
}