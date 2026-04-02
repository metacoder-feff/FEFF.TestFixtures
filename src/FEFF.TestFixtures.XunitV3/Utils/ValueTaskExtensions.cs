namespace FEFF.Extensions;

//TODO: link nuget

internal static class ValueTaskExtensions
{
    public static void WaitSync(this in ValueTask task)
    {
        if(task.IsCompleted)
            return;

        task.AsTask().Wait();
    }
}