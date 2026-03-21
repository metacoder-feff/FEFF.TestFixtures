using Xunit;
using FEFF.TestFixtures.Core;

namespace FEFF.TestFixtures.Xunit;

/*
    FixtureScopeType.TestCase   - disposed via TestFixturesExtensionAttribute
    FixtureScopeType.Class      - disposed via Hack.RegisterAfterClassHandler()
    FixtureScopeType.Collection - disposed via Hack.RegisterAfterCollectionHandler()
    FixtureScopeType.Assembly   - disposed with entire AssemblyTestTracker
*/

internal sealed class AssemblyTestTracker : IAsyncDisposable
{
    private readonly FixtureManager _container;

    public AssemblyTestTracker()
    {
        var ctx = TestContext.Current;
        var key = GetKey(ctx);
        var added = ctx.KeyValueStorage.TryAdd(key, this);
        if(added == false)
            throw new InvalidOperationException("Another value exists in 'TestContext.KeyValueStorage'.");

        _container = new FixtureManagerBuilder().Build();
    }

    public ValueTask DisposeAsync()
    {
        var ctx = TestContext.Current;
        var key = TryGetKey(ctx);
        if(key is not null)
            ctx.KeyValueStorage.TryRemove(key, out _);

        return _container.DisposeAsync();
    }
    
    public IFixtureScope GetScope(string id)
    {
        return _container.GetScope(id);
    }

    internal ValueTask RemoveScopeAsync(string scopeId)
    {
        return _container.RemoveScopeAsync(scopeId);
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
        if(testAssembly == null)
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