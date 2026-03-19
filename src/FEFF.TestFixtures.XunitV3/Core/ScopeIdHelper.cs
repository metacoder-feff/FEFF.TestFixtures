using Xunit;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit;

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
                throw new InvalidOperationException($"Invalid enum value: '{scopeType}' ({(int)scopeType})")
//TODO: utils EnumMatchException
        };
    }

    internal static string GetTestCaseScopeId(IXunitTest test) => GetScopeId(FixtureScopeType.TestCase, test.UniqueID);
}