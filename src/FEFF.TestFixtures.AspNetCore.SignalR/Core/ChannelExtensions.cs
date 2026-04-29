using System.Threading.Channels;

//TODO: nuget

namespace FEFF.Extensions.Threading;

internal static class ChannelExtensions
{
    /// <summary>
    /// Attempts to read a value from the channel with a timeout.
    /// </summary>
    /// <typeparam name="T">The type of the value to read.</typeparam>
    /// <param name="src">The channel reader.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The read value, or <c>null</c> if the timeout occurred.</returns>
    public static async Task<T?> TryReadAsync<T>(this ChannelReader<T> src, TimeSpan timeout, CancellationToken cancellationToken)
    where T : notnull
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        var timeoutToken = timeoutCts.Token;

        try
        {
            return await src.ReadAsync(timeoutToken).ConfigureAwait(false);
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
