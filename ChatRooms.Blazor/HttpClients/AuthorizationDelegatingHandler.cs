using System.Net.Http.Headers;

namespace ChatRooms.Blazor.HttpClients;

public sealed class AuthorizationDelegatingHandler(AccessTokenStore store)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (store.Token is not null)
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", store.Token);

        return base.SendAsync(request, cancellationToken);
    }
}
