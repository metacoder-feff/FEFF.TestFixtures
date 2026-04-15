using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;

namespace FEFF.TestFixtures.AspNetCore.SignalR;

/// <summary>
/// Extension methods for <see cref="TestServer"/> to create SignalR test clients.
/// </summary>
public static class TestServerExtensions
{
    /// <summary>
    /// Creates a SignalR test client that connects to the specified hub URL on the test server.
    /// </summary>
    /// <param name="server">The test server instance.</param>
    /// <param name="url">The hub endpoint path (e.g., "/hub/chat").</param>
    /// <param name="token">An optional JWT token for authenticating the connection.</param>
    /// <returns>A new <see cref="SignalrTestClient"/> ready for testing.</returns>
    public static SignalrTestClient CreateSignalRClient(this TestServer server, string url, string? token = null)
    {
        var c = new HubConnectionBuilder()
        .WithUrl(
            url,
            o =>
            {
                o.HttpMessageHandlerFactory = _ => server.CreateHandler();
                if (token != null)
                    o.AccessTokenProvider = () => Task.FromResult(token)!;
            }
        )
        .Build();

        return new SignalrTestClient(c);
    }
}
