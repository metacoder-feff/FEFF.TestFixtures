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

public static class TestContextExtensions
{
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