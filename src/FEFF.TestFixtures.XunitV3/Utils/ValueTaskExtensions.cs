namespace FEFF.Extensions;

//TODO: link nuget
//TODO: Remove sync-over-async when xUnit v3 supports fully async teardown

/// <summary>
/// Synchronously waits for a ValueTask to complete.
/// Uses Task.Run offload to prevent synchronization context deadlocks.
/// Use with caution - this is still sync-over-async and should be avoided when possible.
/// </summary>
internal static class ValueTaskExtensions
{
    public static void WaitSync(this in ValueTask valueTask)
    {
        if (valueTask.IsCompletedSuccessfully)
            return;

        // Extract task before lambda to avoid in-parameter capture issue
        var task = valueTask.AsTask();

        // Offload to thread pool to prevent sync context capture deadlock
        Task.Run(async () => await task.ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}
