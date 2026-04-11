using FEFF.TestFixtures.Xunit;

namespace Xunit.v3;

/*
XUnit pipeline:
create XunitTestCase[]
---
foreach (var testCase in testCasesToRun)
---
 create test-class instance
  IBeforeAfterTestAttribute.Before
    RUN TEST
  IBeforeAfterTestAttribute.After
 test-class.DisposeAsync()
---
foreach (var testCase in testCasesToRun)
---
-> XunitTestCase.DisposeAsync()
-> DisposalTracker.DisposeAsync()
*/

/// <summary>
/// Extension methods for <see cref="ITestContext"/> to resolve FEFF.TestFixtures fixtures in xUnit v3.
/// </summary>
public static class TestContextExtensions
{
    /// <summary>
    /// Resolves a fixture from the specified scope within the test context.
    /// </summary>
    /// <typeparam name="T">The type of fixture to resolve.</typeparam>
    /// <param name="ctx">The current xUnit test context.</param>
    /// <param name="scopeType">The lifetime scope for the fixture. Defaults to <see cref="FixtureScopeType.TestCase"/>.</param>
    /// <returns>The resolved fixture instance.</returns>
    public static T GetFeffFixture<T>(this ITestContext ctx, FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        if (scopeType == FixtureScopeType.Class)
            HackingTeardownRegistrar.RegisterAfterClassHandler(ctx);
        else if (scopeType == FixtureScopeType.Collection)
            HackingTeardownRegistrar.RegisterAfterCollectionHandler(ctx);

        var scopeId = ScopeIdHelper.GetScopeId(ctx, scopeType);

        return AssemblyTestTracker
            .GetCurrentTracker(ctx)
            .GetScope(scopeId)
            .GetFixture<T>();
    }
}