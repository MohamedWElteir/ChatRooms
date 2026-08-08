using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ChatRooms.Blazor.HttpClients;

public sealed class UserContextCircuitHandler(
    IHttpContextAccessor httpContextAccessor,
    UserContext userContext) : CircuitHandler
{
    public override async Task OnCircuitOpenedAsync(
        Circuit circuit, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        var authentication = await httpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authentication.Succeeded)
            return;

        userContext.SetAccessToken(
            authentication.Properties?.GetTokenValue("access_token"),
            authentication.Properties?.GetTokenValue("expires_at"));
    }
}
