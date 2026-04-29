using FEFF.Extensions;
using Xunit;

namespace FEFF.TestFixtures.Xunit.Internal;

internal static class ScopeIdHelper
{
    internal static string GetScopeId(ITestContext ctx, FixtureScopeType scopeType)
    {
        var id = GetId(scopeType, ctx);
        return GetScopeId(scopeType, id);
    }

    private static string GetScopeId(FixtureScopeType scopeType, string id) => $"{scopeType}-{id}";

    private static string GetId(FixtureScopeType scopeType, ITestContext testContext)
    {
        return scopeType switch
        {
            FixtureScopeType.TestCase   => ThrowHelper.EnsureNotNull(testContext.Test).UniqueID,
            FixtureScopeType.Class      => ThrowHelper.EnsureNotNull(testContext.TestClass).UniqueID,
            FixtureScopeType.Collection => ThrowHelper.EnsureNotNull(testContext.TestCollection).UniqueID,
            FixtureScopeType.Assembly   => ThrowHelper.EnsureNotNull(testContext.TestAssembly).UniqueID,
            _ =>
                throw EnumMatchException.From(scopeType)
        };
    }

    // internal static string GetTestCaseScopeId(IXunitTest test) => GetScopeId(FixtureScopeType.TestCase, test.UniqueID);
}