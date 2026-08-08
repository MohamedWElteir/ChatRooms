using System.Net.Http.Headers;

namespace ChatRooms.Blazor.HttpClients;

public sealed class AuthorizationDelegatingHandler(UserContext userContext)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (userContext.HasValidAccessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", userContext.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
