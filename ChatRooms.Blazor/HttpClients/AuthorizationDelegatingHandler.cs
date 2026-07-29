using System.Net.Http.Headers;

namespace ChatRooms.Blazor.HttpClients;

public sealed class AuthorizationDelegatingHandler(
    IHttpContextAccessor httpContextAccessor,
    UserContext userContext,
    TokenStore tokenStore)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sub = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
               ?? userContext.Sub;

        if (sub is not null)
        {
            var token = tokenStore.Get(sub);
            if (token is not null)
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
