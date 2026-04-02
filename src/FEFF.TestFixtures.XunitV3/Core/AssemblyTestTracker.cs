using Xunit;

namespace FEFF.TestFixtures.Xunit;
using Engine;

/*
    FixtureScopeType.TestCase   - disposed via TestFixturesExtensionAttribute
    FixtureScopeType.Class      - disposed via Hack.RegisterAfterClassHandler()
    FixtureScopeType.Collection - disposed via Hack.RegisterAfterCollectionHandler()
    FixtureScopeType.Assembly   - disposed with entire AssemblyTestTracker
*/

internal sealed class AssemblyTestTracker : IAsyncDisposable
{
    // Defer Fixture Discovery until first test requests it
    private readonly Lazy<FixtureManager> _container = new(CreateMgr);

    private static FixtureManager CreateMgr()
    {
        return new FixtureManagerBuilder().Build();
    }

    private FixtureManager Container => _container.Value;

    public AssemblyTestTracker()
    {
        var ctx = TestContext.Current;
        var key = GetKey(ctx);
        var added = ctx.KeyValueStorage.TryAdd(key, this);
        if (added == false)
            throw new InvalidOperationException("Another value exists in 'TestContext.KeyValueStorage'.");
    }

    public ValueTask DisposeAsync()
    {
        var ctx = TestContext.Current;
        var key = TryGetKey(ctx);
        if (key is not null)
            ctx.KeyValueStorage.TryRemove(key, out _);

        // dispose both: contaner & assemblyFxteScope
        return Container.DisposeAsync();
    }

    public IFixtureScope GetScope(string id)
    {
        return Container.GetScope(id);
    }

    internal ValueTask RemoveScopeAsync(string scopeId)
    {
        return Container.RemoveScopeAsync(scopeId);
    }

    #region static

    private static string GetKey(ITestContext ctx)
    {
        var testAssembly = ctx.TestAssembly;
        ThrowHelper.Assert(testAssembly is not null);

        return $"{nameof(AssemblyTestTracker)}-{testAssembly.UniqueID}";
    }

    private static string? TryGetKey(ITestContext ctx)
    {
        var testAssembly = ctx.TestAssembly;
        if (testAssembly == null)
            return null;

        return $"{nameof(AssemblyTestTracker)}-{testAssembly.UniqueID}";
    }

    internal static AssemblyTestTracker GetCurrentTracker(ITestContext ctx)
    {
        var key = GetKey(ctx);
        _ = ctx.KeyValueStorage.TryGetValue(key, out var res);

        return res as AssemblyTestTracker
            ?? throw new InvalidOperationException($"Extension is not registered. Use '[assembly: {nameof(TestFixturesExtensionAttribute)}]'.");
    }
    #endregion
}