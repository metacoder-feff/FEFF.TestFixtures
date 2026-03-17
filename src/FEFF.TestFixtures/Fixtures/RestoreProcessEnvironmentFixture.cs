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
public sealed class RestoreProcessEnvironmentFixture : IDisposable
{
    private readonly static object __lockObj = new();
    //private readonly static Lock __lockObj = new();

    //Disallow parallel env saving or restoring Process-wide
    private static volatile Env? __oldEnv;
    
    public  RestoreProcessEnvironmentFixture()
    {
        lock(__lockObj)
        {
            if (__oldEnv != null)
                throw new InvalidOperationException($"Can't use {nameof(RestoreProcessEnvironmentFixture)} in parallel tests. For Xunit consider using [Collection] attribute to all the test classes that will be part of a collection. Tests within the same collection run sequentially.");
                
            __oldEnv = EnvironmentHelper.GetEnvironmentVariables();
        }
    }

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