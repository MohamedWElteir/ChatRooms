using System.Net.Http.Headers;

namespace ChatRooms.Blazor.HttpClients;

public sealed class AuthorizationDelegatingHandler(
    UserContext userContext,
    AccessTokenRefresher accessTokenRefresher) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await accessTokenRefresher.GetValidAccessTokenAsync(
            userContext, cancellationToken);

        if (accessToken is null)
        {
            userContext.MarkSessionExpired();
            throw new UnauthorizedAccessException(
                "The user session is expired. Authenticate again before calling the API.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}