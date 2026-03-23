using FEFF.TestFixtures.Core;
using TUnit.Core.Interfaces;

namespace FEFF.TestFixtures.TUnit;

public enum FixtureScopeType { TestCase, Class, Assembly, Session };

public static class DisposeHelper
{
    
}



// ctor/dispose not called without tests
public static class GlobalHooks
{
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

    private static string GetScopeId(Context ctx, FixtureScopeType scopeType)
    {
        var tc = (TestContext)ctx;
        return $"{scopeType}-{tc.Id}";
    }

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

    private static async Task RemoveScope(Context ctx, FixtureManager? manager, FixtureScopeType sc)
    {
        if (manager == null)
            return;

        var id = GetScopeId(ctx, sc);

        await manager.RemoveScopeAsync(id);
    }

    [AfterEvery(Test)]
    public async static Task After(TestContext ctx)
    {
        await RemoveScope(ctx, _manager, FixtureScopeType.TestCase);
    }

    // [AfterEvery(Class)]
    // public async static Task After(ClassHookContext ctx)
    // {
    //     await RemoveScope(ctx, _manager, FixtureScopeType.Class);
    // }

    // [AfterEvery(Assembly)]
    // public async static Task After(AssemblyHookContext ctx)
    // {
    //     await RemoveScope(ctx, _manager, FixtureScopeType.Assembly);
    // }

    // [After(TestSession)]
    // public async static Task After(TestSessionContext ctx)
    // {
    //     var m = Interlocked.Exchange(ref _manager, null);

    //     await RemoveScope(ctx, m, FixtureScopeType.Session);
        
    //     if(m != null)
    //         await m.DisposeAsync();
    // }
}

//TODO: not called
/*
public sealed class LoggingHookExecutor : IHookExecutor, IDisposable
{
    public LoggingHookExecutor()
    {
        Console.WriteLine($"LoggingHookExecutor");
    }

    public async ValueTask ExecuteBeforeTestDiscoveryHook(MethodMetadata hookMethodInfo, BeforeTestDiscoveryContext context, Func<ValueTask> action)
    {
        Console.WriteLine($"Before test discovery hook: {hookMethodInfo.Name}");
        await action();
    }

    public async ValueTask ExecuteBeforeTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action)
    {
        Console.WriteLine($"Before test session hook: {hookMethodInfo.Name}");
        await action();
    }

    public async ValueTask ExecuteBeforeAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action)
    {
        Console.WriteLine($"Before assembly hook: {hookMethodInfo.Name}");
        await action();
    }

    public async ValueTask ExecuteBeforeClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action)
    {
        Console.WriteLine($"Before class hook: {hookMethodInfo.Name} for class {context.ClassType.Name}");

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hook failed: {ex.Message}");
            throw;
        }
    }

    public async ValueTask ExecuteBeforeTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action)
    {
        Console.WriteLine($"Before test hook: {hookMethodInfo.Name} for test {context.Metadata.TestName}");
        await action();
    }

    public async ValueTask ExecuteAfterTestDiscoveryHook(MethodMetadata hookMethodInfo, TestDiscoveryContext context, Func<ValueTask> action)
    {
        await action();
        Console.WriteLine($"After test discovery hook: {hookMethodInfo.Name}");
    }

    public async ValueTask ExecuteAfterTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action)
    {
        await action();
        Console.WriteLine($"After test session hook: {hookMethodInfo.Name}");
    }

    public async ValueTask ExecuteAfterAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action)
    {
        await action();
        Console.WriteLine($"After assembly hook: {hookMethodInfo.Name}");
    }

    public async ValueTask ExecuteAfterClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action)
    {
        await action();
        Console.WriteLine($"After class hook: {hookMethodInfo.Name} for class {context.ClassType.Name}");
    }

    public async ValueTask ExecuteAfterTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action)
    {
        await action();
        Console.WriteLine($"After test hook: {hookMethodInfo.Name} for test {context.Metadata.TestName}");
    }

    public void Dispose()
    {
        Console.WriteLine($"Dispose");
    }
}
*/