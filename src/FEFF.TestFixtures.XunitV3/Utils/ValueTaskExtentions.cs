namespace FEFF.Extentions;

//TODO: link nuget

internal static class ValueTaskExtentions
{
    public static void WaitSync(this in ValueTask task)
    {
        if(task.IsCompleted)
            return;

        task.AsTask().Wait();
    }
}