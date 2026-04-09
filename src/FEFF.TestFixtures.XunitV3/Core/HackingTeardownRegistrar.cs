using Xunit;
using Xunit.v3;
using FEFF.Extensions.Reflection;
using Xunit.Sdk;

namespace FEFF.TestFixtures.Xunit;

internal class ClassScopeAdapter : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        return ScopeRemoverHelper.RemoveCurrentClassScope();
    }
}

internal class CollectionScopeAdapter : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        return ScopeRemoverHelper.RemoveCurrentCollectionScope();
    }
}

internal static class HackingTeardownRegistrar
{
    internal static void RegisterAfterClassHandler(ITestContext ctx) =>
        RegisterDisposable<ClassScopeAdapter>(ctx, "class");

    internal static void RegisterAfterCollectionHandler(ITestContext ctx) =>
        RegisterDisposable<CollectionScopeAdapter>(ctx, "collection");

    private static void RegisterDisposable<T>(ITestContext ctx, string category)
    {
        var mapping = GetFixtureMappingManager(ctx, category);

        if(mapping == null)
            throw new NotSupportedException($"Current version of extension does not support teardown of '{category}' scope.");

        var type = typeof(T);
        if(mapping.LocalFixtureTypes.Contains(type))
            return;

        // otherwise WaitSync() can deadlock
        ThrowHelper.Assert(type.Implements(typeof(IAsyncLifetime)) == false);
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
        obj.TryGetPrivateInstanceFieldValue<FixtureMappingManager>("parentMappingManager");

    private static FixtureMappingManager? GetFixtureMappingManager(ITestContext obj) =>
        obj.TryGetPrivateInstanceFieldValue<FixtureMappingManager>("fixtures");

    private static string? GetFixtureCategory(FixtureMappingManager obj) =>
        obj.TryGetPrivateInstanceFieldValue<string>("fixtureCategory");
}