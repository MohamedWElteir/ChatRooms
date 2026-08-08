using System.Net.Http.Headers;

namespace ChatRooms.Blazor.HttpClients;

public sealed class AuthorizationDelegatingHandler(
    UserContext userContext,
    AccessTokenRefresher accessTokenRefresher) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (userContext.HasValidAccessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", userContext.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }

        if (userContext.RefreshToken is not null)
        {
            var refreshed = await accessTokenRefresher.RefreshAsync(
                userContext.RefreshToken, cancellationToken);

            if (refreshed is not null)
            {
                userContext.Initialize(
                    userContext.Sub,
                    refreshed.AccessToken,
                    refreshed.RefreshToken ?? userContext.RefreshToken,
                    refreshed.AccessTokenExpiresAt);

                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", userContext.AccessToken);
                return await base.SendAsync(request, cancellationToken);
            }

            userContext.MarkSessionExpired();
        }

        throw new UnauthorizedAccessException(
            "The user session is expired. Authenticate again before calling the API.");
    }
}