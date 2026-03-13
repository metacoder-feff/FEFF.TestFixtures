using Xunit;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit;

internal static class ScopeRemoverHelper
{
    internal static ValueTask RemoveCurrentClassScope() => RemoveCurrentScope(FixtureScopeType.Class);
    internal static ValueTask RemoveCurrentCollectionScope() => RemoveCurrentScope(FixtureScopeType.Collection);

    private static ValueTask RemoveCurrentScope(FixtureScopeType scope)
    {
        var ctx = TestContext.Current;
        var tracker = AssemblyTestTracker.GetCurrentTracker(ctx);
        var scopeId = ScopeIdHelper.GetScopeId(ctx, scope);

        return tracker.RemoveScopeAsync(scopeId);
    }

    internal static void RemoveTestCaseScope(IXunitTest test)
    {
        var ctx = TestContext.Current;
        var tracker = AssemblyTestTracker.GetCurrentTracker(ctx);
        var scopeId = ScopeIdHelper.GetTestCaseScopeId(test);

//TODO: async (Xunit proposal https://github.com/xunit/xunit/issues/3504)
        tracker.RemoveScopeAsync(scopeId).WaitSync();
    }
}