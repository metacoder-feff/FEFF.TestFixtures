using FEFF.Extensions;
using FEFF.TestFixtures.Engine;
using Xunit;
using Xunit.v3;

namespace FEFF.TestFixtures.Xunit.Internal;

/// <summary>
/// Adapter that integrates FEFF.TestFixtures with xUnit v4 lifecycle events.
/// Manages the FixtureManager and handles scope creation/cleanup for test assemblies,
/// collections, classes, and test cases.
/// </summary>
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

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return _fixtureManager.DisposeAsync();
    }

    /// <inheritdoc/>
    public ValueTask OnTestAssemblyFinishedAsync(IXunitTestAssembly testAssembly)
    {
        ArgumentNullException.ThrowIfNull(testAssembly);
        var id = ScopeIdHelper.GetScopeId(testAssembly);
        return _fixtureManager.RemoveScopeAsync(id);
    }

    #region EventHandlers
    /// <inheritdoc/>
    public ValueTask OnTestCollectionFinishedAsync(IXunitTestCollection testCollection)
    {
        ArgumentNullException.ThrowIfNull(testCollection);
        var id = ScopeIdHelper.GetScopeId(testCollection);
        return _fixtureManager.RemoveScopeAsync(id);
    }

    /// <inheritdoc/>
    public ValueTask OnTestClassFinishedAsync(IXunitTestClass testClass)
    {
        ArgumentNullException.ThrowIfNull(testClass);
        var id = ScopeIdHelper.GetScopeId(testClass);
        return _fixtureManager.RemoveScopeAsync(id);
    }

    /// <inheritdoc/>
    public ValueTask OnTestCaseFinishedAsync(IXunitTestCase testCase)
    {
        ArgumentNullException.ThrowIfNull(testCase);
        var id = ScopeIdHelper.GetScopeId(testCase);
        return _fixtureManager.RemoveScopeAsync(id);
    }

    /// <inheritdoc/>
    public ValueTask OnTestAssemblyStartingAsync(IXunitTestAssembly testAssembly) => ValueTask.CompletedTask;
    
    /// <inheritdoc/>
    public ValueTask OnTestCaseStartingAsync(IXunitTestCase testCase) => ValueTask.CompletedTask;
    
    /// <inheritdoc/>
    public ValueTask OnTestClassStartingAsync(IXunitTestClass testClass) => ValueTask.CompletedTask;
    
    /// <inheritdoc/>
    public ValueTask OnTestCollectionStartingAsync(IXunitTestCollection testCollection) => ValueTask.CompletedTask;
    #endregion

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
