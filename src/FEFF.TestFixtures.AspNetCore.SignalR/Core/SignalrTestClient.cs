using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR.Client;

namespace FEFF.TestFixtures.AspNetCore.Preview;

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

public sealed class SignalrTestClient : IAsyncDisposable
{
    private readonly Channel<ServerEvent> _eventsQueue = Channel.CreateUnbounded<ServerEvent>();
    private readonly HubConnection _connection;

    // internal because owns HubConnection
    internal SignalrTestClient(HubConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// The client must define a signature for the method called by the Hub (due to the HubConnection implementation).
    /// </summary>
    /// <param name="expectedMethodName">The method name called by the Hub.</param>
    /// <param name="expectedArgsCount">The number of arguments sent from the Hub. The types of arguments do not matter for testing.</param>
    public void Subscribe(string expectedMethodName, int expectedArgsCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(expectedArgsCount, 0);

        var types = Type.EmptyTypes;
        if(expectedArgsCount > 0)
            types = Enumerable.Repeat(typeof(object), expectedArgsCount).ToArray();

        _ = _connection.On(expectedMethodName, types, SignalRHandler, expectedMethodName);

//TODO: handle all args?
        // only one mapping per 'methodName' works
        // https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/common/Protocols.Json/src/Protocol/JsonHubProtocol.cs#L867
        // throw new InvalidDataException($"Invocation provides {paramIndex} argument(s) but target expects {paramCount}.");
    }

    private async Task SignalRHandler(object?[] args, object methodName)
    {
        var e = new ServerEvent(
            Method: methodName as string,
            Args: args
        );
        await _eventsQueue.Writer.WriteAsync(e);
    }

    public async Task<ServerEvent?> WaitForEvent(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return await _eventsQueue.Reader.TryReadAsync(timeout, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    public async Task StartAsync()
    {
        await _connection.StartAsync();
    }
}

public record ServerEvent(string? Method, object?[] Args);