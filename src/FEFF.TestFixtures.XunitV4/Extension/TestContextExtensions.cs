using FEFF.TestFixtures.Xunit.V4;
using FEFF.TestFixtures.Xunit.V4.Internal;

namespace Xunit.v3;

/// <summary>
/// Extension methods for <see cref="ITestContext"/> to resolve FEFF.TestFixtures fixtures in xUnit v3.
/// </summary>
public static class TestContextExtensionsV4
{
    /// <summary>
    /// Resolves a fixture from the specified scope within the test context.
    /// </summary>
    /// <typeparam name="T">The type of fixture to resolve.</typeparam>
    /// <param name="ctx">The current xUnit test context.</param>
    /// <param name="scopeType">The lifetime scope for the fixture. Defaults to <see cref="FixtureScopeType.TestCase"/>.</param>
    /// <returns>The resolved fixture instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the extension is not properly registered.</exception>
    public static T GetFeffFixture<T>(this ITestContext ctx, FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        var scopeId = ScopeIdHelper.GetScopeId(ctx, scopeType);

        return FixtureAdapter
            .GetCurrent(ctx)
            .GetScope(scopeId)
            .GetFixture<T>();
    }
}
