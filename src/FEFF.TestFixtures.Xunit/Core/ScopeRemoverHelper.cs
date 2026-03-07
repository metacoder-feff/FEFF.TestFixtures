using Xunit;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit;

internal static class ScopeRemoverHelper
{
    internal static async ValueTask RemoveCurrentClassScope() => await RemoveCurrentScope(FixtureScopeType.Class);
    internal static async ValueTask RemoveCurrentCollectionScope() => await RemoveCurrentScope(FixtureScopeType.Collection);

    private static async Task RemoveCurrentScope(FixtureScopeType scope)
    {
        var ctx = TestContext.Current;
        var tracker = AssemblyTestTracker.GetCurrentTracker(ctx);
        var scopeId = ScopeIdHelper.GetScopeId(ctx, scope);

        await tracker.RemoveScopeAsync(scopeId);
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