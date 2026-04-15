using System.Collections.Frozen;

namespace FEFF.TestFixtures;

using Env = FrozenDictionary<string, string>;

/// <summary>
/// Snapshots the process environment before a test and reverts changes after the test.
/// </summary>
/// <remarks>
/// These tests should not be run in parallel; otherwise, an exception will be thrown.
/// xUnit: Consider using the [Collection] attribute on all test classes that will be part of a collection.
/// Tests within the same collection run sequentially.
/// </remarks>
[Fixture]
public sealed class EnvironmentFixture : IDisposable
{
#if NET9_0_OR_GREATER
    private static readonly Lock __lockObj = new(); 
#else
    private static readonly Object __lockObj = new();
#endif

    // Disallow parallel environment variable saving or restoring process-wide
    private static volatile Env? __oldEnv;

    /// <summary>
    /// Gets a snapshot of the environment variables as they were when this fixture was constructed.
    /// </summary>
    public Env InitialSnapshot { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EnvironmentFixture"/> and captures the current
    /// environment variables.
    /// </summary>
    /// <remarks>
    /// Throws if another instance is already active in the process.
    /// </remarks>
    public EnvironmentFixture()
    {
        lock (__lockObj)
        {
            if (__oldEnv != null)
                throw new InvalidOperationException($"'{nameof(EnvironmentFixture)}' mutates process environment and cannot be used in parallel. Ensure tests using this fixture run sequentially. For xUnit, consider using the [Collection] attribute on all test classes that will be part of a collection. Tests within the same collection run sequentially.");

            InitialSnapshot = EnvironmentHelper.GetEnvironmentVariables();

            __oldEnv = InitialSnapshot;
        }
    }

    /// <summary>
    /// Same as <see cref="Environment.SetEnvironmentVariable(string, string?)"/>.<br/>
    /// This method is used not to forget to instantiate <see cref="EnvironmentFixture"/>.
    /// </summary>
    /// <remarks>
    /// Underling <see cref="Environment.SetEnvironmentVariable(string, string?)"/> can be also be used instad of this.
    /// </remarks>
    /// <param name="variable">The name of the environment variable.</param>
    /// <param name="value">The value to set for the environment variable.</param>
#pragma warning disable CA1822 // Mark members as static
    public void SetEnvironmentVariable(string variable, string? value)
    {
        Environment.SetEnvironmentVariable(variable, value);
    }
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Restores all process environment variables to their original state captured during construction.
    /// </summary>
    public void Dispose()
    {
        lock (__lockObj)
        {
            if (__oldEnv == null)
                return;

            var newEnv = EnvironmentHelper.GetEnvironmentVariables();

            RevertOldValues(__oldEnv, newEnv);
            RemoveNewValues(__oldEnv, newEnv);

            __oldEnv = null;
        }
    }

    private static void RevertOldValues(Env oldEnv, Env newEnv)
    {
        foreach (var oldKvp in oldEnv)
        {
            string? newValue = newEnv.TryGetOrNull(oldKvp.Key);

            if (oldKvp.Value != newValue)
                Environment.SetEnvironmentVariable(oldKvp.Key, oldKvp.Value);
        }
    }

    private static void RemoveNewValues(Env oldEnv, Env newEnv)
    {
        foreach (var k in newEnv.Keys)
        {
            if (oldEnv.ContainsKey(k) == false)
                Environment.SetEnvironmentVariable(k, null);
        }
    }
}
