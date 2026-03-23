namespace ExampleTests.TUnit;

public class GlobalHooks
{
    [BeforeEvery(Class)]
    public static Task BeforeC(ClassHookContext ctx)
    {
        return Task.CompletedTask;
    }
    
    [BeforeEvery(Assembly)]
    public static Task BeforeA(AssemblyHookContext ctx)
    {
        return Task.CompletedTask;
    }

    [AfterEvery(Class)]
    public static Task AfterC(ClassHookContext ctx)
    {
        return Task.CompletedTask;
    }

    [AfterEvery(Assembly)]
    public static Task AfterA(AssemblyHookContext ctx)
    {
        return Task.CompletedTask;
    }

}