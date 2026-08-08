using System.Net;

namespace ChatRooms.Blazor.Tests.HttpClients;

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    private int _requestCount;

    public int RequestCount => Volatile.Read(ref _requestCount);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _requestCount);
        return await responder(request);
    }
}

internal sealed class StubHttpClientFactory(StubHttpMessageHandler handler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(handler)
    {
        BaseAddress = new Uri("https://keycloak.local")
    };
}

internal static class KeycloakResponses
{
    public static HttpResponseMessage Success(string accessToken, string? refreshToken = null)
    {
        var refreshPart = refreshToken is null
            ? string.Empty
            : $",\"refresh_token\":\"{refreshToken}\"";

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                $"{{\"access_token\":\"{accessToken}\",\"expires_in\":900{refreshPart}}}",
                System.Text.Encoding.UTF8,
                "application/json")
        };
    }

    public static HttpResponseMessage Failure() => new(HttpStatusCode.Unauthorized);
}