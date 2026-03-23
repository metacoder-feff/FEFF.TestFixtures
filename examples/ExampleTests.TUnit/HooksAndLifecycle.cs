using TUnit.Core.Executors;
using TUnit.Core.Interfaces;

[assembly: HookExecutor<ExampleTests.TUnit.LoggingHookExecutor>]

namespace ExampleTests.TUnit;

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