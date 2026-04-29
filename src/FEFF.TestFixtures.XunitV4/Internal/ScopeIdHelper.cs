using FEFF.Extensions;
using Xunit;
using Xunit.Sdk;

namespace FEFF.TestFixtures.Xunit.V4.Internal;

internal static class ScopeIdHelper
{
    internal static string GetScopeId(ITestContext testContext, FixtureScopeType scopeType)
    {
        return scopeType switch
        {
            FixtureScopeType.TestCase   => GetScopeId(ThrowHelper.EnsureNotNull(testContext.TestCase)),
            FixtureScopeType.Class      => GetScopeId(ThrowHelper.EnsureNotNull(testContext.TestClass)),
            FixtureScopeType.Collection => GetScopeId(ThrowHelper.EnsureNotNull(testContext.TestCollection)),
            FixtureScopeType.Assembly   => GetScopeId(ThrowHelper.EnsureNotNull(testContext.TestAssembly)),
            _ =>
                throw EnumMatchException.From(scopeType)
        };
    }

    internal static string GetScopeId(ITestCaseMetadata testCase) => 
        GetScopeId(FixtureScopeType.TestCase, testCase.UniqueID);
    internal static string GetScopeId(ITestClassMetadata testClass) => 
        GetScopeId(FixtureScopeType.Class, testClass.UniqueID);
    internal static string GetScopeId(ITestCollectionMetadata testCollection) => 
        GetScopeId(FixtureScopeType.Collection, testCollection.UniqueID);
    internal static string GetScopeId(IAssemblyMetadata testAssembly) => 
        GetScopeId(FixtureScopeType.Assembly, testAssembly.UniqueID);

    private static string GetScopeId(FixtureScopeType scopeType, string id) => $"{scopeType}-{id}";
}
