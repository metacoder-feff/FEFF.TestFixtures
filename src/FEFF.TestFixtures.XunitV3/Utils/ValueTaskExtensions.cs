namespace FEFF.Extensions;

//TODO: link nuget

/// <summary>
/// Use with caution! May deadlock!
/// </summary>
internal static class ValueTaskExtensions
{
    public static void WaitSync(this in ValueTask valueTask)
    {
        if(valueTask.IsCompletedSuccessfully)
            return;

        var waitTask = valueTask.AsTask();

        waitTask.ConfigureAwait(false).GetAwaiter().GetResult();

        // var t = Task.Run(async () => await waitTask.ConfigureAwait(false) );
        // t.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}