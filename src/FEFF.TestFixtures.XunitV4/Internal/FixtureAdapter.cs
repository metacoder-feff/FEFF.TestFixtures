using FEFF.Extensions;
using FEFF.TestFixtures.Engine;
using Xunit;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit.Internal;

internal class FixtureAdapter 
    : INotifyTestAssemblyLifecycleAsync
    , INotifyTestCollectionLifecycleAsync
    , INotifyTestClassLifecycleAsync
    , INotifyTestCaseLifecycleAsync
    , IAsyncDisposable
{
    private readonly FixtureManager _fixtureManager;

    public FixtureAdapter()
    {
        _fixtureManager = new FixtureManagerBuilder().Build();
        SetCurrent(TestContext.Current, this);
    }

    public ValueTask DisposeAsync()
    {
        return _fixtureManager.DisposeAsync();
    }

    public ValueTask OnTestAssemblyFinishedAsync(IXunitTestAssembly testAssembly)
    {
        // TODO: use arg
        var id = ScopeIdHelper.GetScopeId(TestContext.Current, FixtureScopeType.Assembly);
        return _fixtureManager.RemoveScopeAsync(id);
    }
    public ValueTask OnTestCollectionFinishedAsync(IXunitTestCollection testCollection)
    {
        // TODO: use arg
        var id = ScopeIdHelper.GetScopeId(TestContext.Current, FixtureScopeType.Collection);
        return _fixtureManager.RemoveScopeAsync(id);
    }
    public ValueTask OnTestClassFinishedAsync(IXunitTestClass testClass)
    {
        // TODO: use arg
        var id = ScopeIdHelper.GetScopeId(TestContext.Current, FixtureScopeType.Class);
        return _fixtureManager.RemoveScopeAsync(id);
    }
    public ValueTask OnTestCaseFinishedAsync(IXunitTestCase testCase)
    {
        // TODO: use arg
        var id = ScopeIdHelper.GetScopeId(TestContext.Current, FixtureScopeType.TestCase);
        return _fixtureManager.RemoveScopeAsync(id);
    }

    public ValueTask OnTestAssemblyStartingAsync(IXunitTestAssembly testAssembly) => ValueTask.CompletedTask;
    public ValueTask OnTestCaseStartingAsync(IXunitTestCase testCase) => ValueTask.CompletedTask;
    public ValueTask OnTestClassStartingAsync(IXunitTestClass testClass) => ValueTask.CompletedTask;
    public ValueTask OnTestCollectionStartingAsync(IXunitTestCollection testCollection) => ValueTask.CompletedTask;

    internal IFixtureScope GetScope(string scopeId)
    {
        return _fixtureManager.GetScope(scopeId);
    }

    #region static

    private static void SetCurrent(ITestContext ctx, FixtureAdapter obj)
    {
        var key = GetKey(ctx);
        var added = ctx.KeyValueStorage.TryAdd(key, obj);
        if (added == false)
            throw new InvalidOperationException("Another value exists in 'TestContext.KeyValueStorage'.");
    }

    private static string GetKey(ITestContext ctx)
    {
        var testAssembly = ctx.TestAssembly;
        ThrowHelper.Assert(testAssembly is not null);

        return $"{nameof(FixtureAdapter)}-{testAssembly.UniqueID}";
    }

    internal static FixtureAdapter GetCurrent(ITestContext ctx)
    {
        var key = GetKey(ctx);
        _ = ctx.KeyValueStorage.TryGetValue(key, out var res);

        return res as FixtureAdapter
            ?? throw new InvalidOperationException($"Extension is not registered. Use '[assembly: {nameof(TestFixturesExtensionAttribute)}]'.");
    }
    #endregion
}