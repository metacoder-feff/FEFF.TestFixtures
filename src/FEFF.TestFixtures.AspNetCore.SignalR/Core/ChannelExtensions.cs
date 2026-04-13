using System.Threading.Channels;

//TODO: nuget

namespace FEFF.Extensions.Threading;

internal static class ChannelExtensions
{
    public static async Task<T?> TryReadAsync<T>(this ChannelReader<T> src, TimeSpan timeout, CancellationToken cancellationToken)
    where T : notnull
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        var timeoutToken = timeoutCts.Token;
        
        try
        {
            return await src.ReadAsync(timeoutToken);
        }
        catch (OperationCanceledException e) 
        when (e.CancellationToken == timeoutToken 
            && timeoutCts.IsCancellationRequested == true
            && cancellationToken.IsCancellationRequested == false)
        {
            return default;
        }
    }
}
