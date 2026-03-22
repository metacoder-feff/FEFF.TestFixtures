using Xunit;
using Xunit.v3;
using FEFF.Extentions.Reflection;

namespace FEFF.TestFixtures.Xunit;

internal class TestClassAdapter : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        return ScopeRemoverHelper.RemoveCurrentClassScope();
    }
}

internal class TestCollectionAdapter : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        return ScopeRemoverHelper.RemoveCurrentCollectionScope();
    }
}

internal static class Hack
{
    internal static void RegisterAfterClassHandler(ITestContext ctx) => 
        RegisterDisposable<TestClassAdapter>(ctx, "class");

    internal static void RegisterAfterCollectionHandler(ITestContext ctx) => 
        RegisterDisposable<TestCollectionAdapter>(ctx, "collection");

    private static void RegisterDisposable<T>(ITestContext ctx, string category)
    {
        var mapping = GetFixtureMappingManager(ctx, category);

        if(mapping == null)
            throw new NotSupportedException($"Current version of extension does not support teardown of '{category}' scope.");

        var type = typeof(T);
        if(mapping.LocalFixtureTypes.Contains(type))
            return;

        mapping.InitializeAsync(type).WaitSync();
    }

    private static FixtureMappingManager? GetFixtureMappingManager(ITestContext ctx, string category)
    {
        var mapping = GetFixtureMappingManager(ctx);

        return MappingOrParent(mapping, category);
    }

    private static FixtureMappingManager? MappingOrParent(FixtureMappingManager? mapping, string category)
    {
        if(mapping == null)
            return null;

        var cat = GetFixtureCategory(mapping);
        if(cat != null && cat.Equals(category, StringComparison.InvariantCultureIgnoreCase))
            return mapping;

        var parent = GetParent(mapping);

        return MappingOrParent(parent, category);
    }

    private static FixtureMappingManager? GetParent(FixtureMappingManager obj) => 
        obj.TryGetPrivateInstaceFieldValue<FixtureMappingManager>("parentMappingManager");

    private static FixtureMappingManager? GetFixtureMappingManager(ITestContext obj) => 
        obj.TryGetPrivateInstaceFieldValue<FixtureMappingManager>("fixtures");

    private static string? GetFixtureCategory(FixtureMappingManager obj) => 
        obj.TryGetPrivateInstaceFieldValue<string>("fixtureCategory");
}