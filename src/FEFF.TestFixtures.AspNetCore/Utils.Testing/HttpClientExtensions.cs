using System.Net.Http.Headers;

namespace FEFF.Extensions;

internal static class HttpClientExtensions
{
    // RFC 7235
    public const string BearerAuthHeader = "Bearer";

    public static void AddBearerHeader(this HttpClient client, string accessToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerAuthHeader, accessToken);
    }
}