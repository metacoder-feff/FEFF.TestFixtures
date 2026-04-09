using FEFF.Extensions;

namespace FEFF.TestFixtures.TUnit;
using Engine;

public enum FixtureScopeType { TestCase, Class, Assembly, Session };

// ctor/dispose not called without tests
public static class GlobalHooksExtension
{
//TODO: multiple sessions???
// => scopeId
// => dispose

#if NET9_0_OR_GREATER
    private static readonly Lock _lock = new(); 
#else
    private static readonly Object _lock = new(); 
#endif
    private static volatile FixtureManager? _manager;

    public static T GetFeffFixture<T>(this TestContext ctx, FixtureScopeType scopeType = FixtureScopeType.TestCase)
    where T : notnull
    {
        ArgumentNullException.ThrowIfNull(ctx);

        var m = GetManager();
        var id = GetScopeId(ctx, scopeType);
        return m.GetScope(id).GetFixture<T>();
    }
    
    private static string GetScopeId(TestContext ctx, FixtureScopeType scopeType) => scopeType switch
    {
        FixtureScopeType.TestCase   => GetScopeId(ctx),
        FixtureScopeType.Class      => GetScopeId(ctx.ClassContext),
        FixtureScopeType.Assembly   => GetScopeId(ctx.ClassContext.AssemblyContext),
        FixtureScopeType.Session    => GetScopeId(ctx.ClassContext.AssemblyContext.TestSessionContext),
        _ =>
            throw EnumMatchException.From(scopeType)
    };

    private static string GetScopeId(string ctxId, FixtureScopeType scopeType) =>
        $"{scopeType}-{ctxId}";

    private static string GetScopeId(TestContext ctx) =>
        GetScopeId(ctx.Id, FixtureScopeType.TestCase);

    private static string GetScopeId(ClassHookContext ctx) =>
        GetScopeId($"{ctx.ClassType.FullName}_{ctx.ClassType.Assembly.FullName}", FixtureScopeType.Class);

    private static string GetScopeId(AssemblyHookContext ctx) =>
        GetScopeId(ctx.Assembly.FullName!, FixtureScopeType.Assembly);

    private static string GetScopeId(TestSessionContext ctx) =>
        GetScopeId(ctx.Id, FixtureScopeType.Session);

    private static FixtureManager GetManager()
    {
        // double-check: optimization
        if(_manager != null)
            return _manager;

        lock(_lock)
        {
            // double-check: guard
            _manager ??= new FixtureManagerBuilder().Build();
        }
        return _manager;
    }

    private static ValueTask RemoveScope(FixtureManager? manager, string id)
    {
        if (manager == null)
            return ValueTask.CompletedTask;

        return manager.RemoveScopeAsync(id);
    }

    [AfterEvery(Test)]
    public async static Task AfterT(TestContext ctx)
    {
        var id = GetScopeId(ctx);
        await RemoveScope(_manager, id).ConfigureAwait(false);
    }

    [AfterEvery(Class)]
    public async static Task AfterC(ClassHookContext ctx)
    {
        var id = GetScopeId(ctx);
        await RemoveScope(_manager, id).ConfigureAwait(false);
    }

    [AfterEvery(Assembly)]
    public async static Task AfterA(AssemblyHookContext ctx)
    {
        var id = GetScopeId(ctx);
        await RemoveScope(_manager, id).ConfigureAwait(false);
    }

    // Dispose _manager here.
    [After(TestSession)]
    public async static Task AfterS(TestSessionContext ctx)
    {
        var m = Interlocked.Exchange(ref _manager, null);

        var id = GetScopeId(ctx);
        await RemoveScope(m, id).ConfigureAwait(false);
        
        // dispose both: manager & assemblyFixtureScope
        if(m != null)
            await m.DisposeAsync().ConfigureAwait(false);
    }
}