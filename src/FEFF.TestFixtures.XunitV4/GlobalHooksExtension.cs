using FEFF.Extensions;

namespace FEFF.TestFixtures.XunitV4;

using Engine;

/// <summary>
/// Defines the lifetime scope for a fixture in TUnit tests.
/// </summary>
public enum FixtureScopeType
{
    /// <summary>
    /// The fixture is scoped to an individual test case.
    /// </summary>
    TestCase,

    /// <summary>
    /// The fixture is scoped to a test class.
    /// </summary>
    Class,

    /// <summary>
    /// The fixture is scoped to a test assembly.
    /// </summary>
    Assembly,

    /// <summary>
    /// The fixture is scoped to the entire test session.
    /// </summary>
    Session
}

/// <summary>
/// Provides TUnit integration hooks for the FEFF.TestFixtures framework.
/// Automatically manages fixture lifecycles across test, class, assembly, and session scopes.
/// </summary>
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

    /// <summary>
    /// Resolves a fixture from the specified scope within the test context.
    /// </summary>
    /// <typeparam name="T">The type of fixture to resolve.</typeparam>
    /// <param name="ctx">The current TUnit test context.</param>
    /// <param name="scopeType">The lifetime scope for the fixture. Defaults to <see cref="FixtureScopeType.TestCase"/>.</param>
    /// <returns>The resolved fixture instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ctx"/> is <c>null</c>.</exception>
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
        if (_manager != null)
            return _manager;

        lock (_lock)
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

    /// <summary>
    /// TUnit hook executed after every test case. Disposes the test-case-scoped fixtures.
    /// </summary>
    /// <param name="ctx">The test context.</param>
    [AfterEvery(Test)]
    public async static Task AfterT(TestContext ctx)
    {
        var id = GetScopeId(ctx);
        await RemoveScope(_manager, id).ConfigureAwait(false);
    }

    /// <summary>
    /// TUnit hook executed after every test class. Disposes the class-scoped fixtures.
    /// </summary>
    /// <param name="ctx">The class hook context.</param>
    [AfterEvery(Class)]
    public async static Task AfterC(ClassHookContext ctx)
    {
        var id = GetScopeId(ctx);
        await RemoveScope(_manager, id).ConfigureAwait(false);
    }

    /// <summary>
    /// TUnit hook executed after every test assembly. Disposes the assembly-scoped fixtures.
    /// </summary>
    /// <param name="ctx">The assembly hook context.</param>
    [AfterEvery(Assembly)]
    public async static Task AfterA(AssemblyHookContext ctx)
    {
        var id = GetScopeId(ctx);
        await RemoveScope(_manager, id).ConfigureAwait(false);
    }

    /// <summary>
    /// TUnit hook executed after the test session. Disposes the session-scoped fixture
    /// and the underlying <see cref="FixtureManager"/>.
    /// </summary>
    /// <param name="ctx">The test session context.</param>
    [After(TestSession)]
    public async static Task AfterS(TestSessionContext ctx)
    {
        var m = Interlocked.Exchange(ref _manager, null);

        var id = GetScopeId(ctx);
        await RemoveScope(m, id).ConfigureAwait(false);

        // dispose both: manager & assemblyFixtureScope
        if (m != null)
            await m.DisposeAsync().ConfigureAwait(false);
    }
}
