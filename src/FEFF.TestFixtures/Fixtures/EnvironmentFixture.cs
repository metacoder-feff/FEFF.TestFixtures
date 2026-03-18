using System.Collections.Frozen;

namespace FEFF.TestFixtures;

using Env = FrozenDictionary<string, string>;

/// <summary>
/// Reverts ProcessEnvironment changes After Test.
/// Those tests should not be run in parallel otherwise the exception would be thrown.
/// Xunit: Consider using [Collection] attribute to all the test classes that will be part of a collection.
/// Tests within the same collection run sequentially.
/// </summary>
[Fixture]
public sealed class EnvironmentFixture : IDisposable
{
#if NET9_0_OR_GREATER
    private static readonly Lock __lockObj = new(); 
#else
    private static readonly Object __lockObj = new(); 
#endif

    //Disallow parallel env saving or restoring Process-wide
    private static volatile Env? __oldEnv;

    public Env InitialSnapshot { get; }

    public  EnvironmentFixture()
    {
        lock(__lockObj)
        {
            if (__oldEnv != null)
                throw new InvalidOperationException($"Can't use {nameof(EnvironmentFixture)} in parallel tests. For Xunit consider using [Collection] attribute to all the test classes that will be part of a collection. Tests within the same collection run sequentially.");
                
            InitialSnapshot = EnvironmentHelper.GetEnvironmentVariables();

            __oldEnv = InitialSnapshot;
        }
    }

    /// <summary>
    /// Same as <see cref="Environment.SetEnvironmentVariable"/>.<br/>
    /// This method is used not to forget to instantiate <see cref="EnvironmentFixture"/>.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public void SetEnvironmentVariable(string variable, string? value)
    {
        Environment.SetEnvironmentVariable(variable, value);
    }
#pragma warning restore CA1822 // Mark members as static

    public void Dispose()
    {
        lock(__lockObj)
        {
            if(__oldEnv == null)
                return;

            var newEnv = EnvironmentHelper.GetEnvironmentVariables();

            RevertOldValues(__oldEnv, newEnv);
            RemoveNewValues(__oldEnv, newEnv);

            __oldEnv = null;
        }
    }

    private static void RevertOldValues(Env oldEnv, Env newEnv)
    {
        foreach(var oldKvp in oldEnv)
        {
            string? newValue = newEnv.TryGetOrNull(oldKvp.Key);

            if (oldKvp.Value != newValue)
                Environment.SetEnvironmentVariable(oldKvp.Key, oldKvp.Value);
        }
    }

    private static void RemoveNewValues(Env oldEnv, Env newEnv)
    {
        foreach(var k in newEnv.Keys)
        {
            if(oldEnv.ContainsKey(k) == false)
                Environment.SetEnvironmentVariable(k, null);
        }
    }
}