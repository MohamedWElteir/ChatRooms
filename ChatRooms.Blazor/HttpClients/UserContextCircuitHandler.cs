using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ChatRooms.Blazor.HttpClients;

public sealed class UserContextCircuitHandler(
    AuthenticationStateProvider authenticationStateProvider,
    ServerSessionStore sessionStore,
    IHttpContextAccessor httpContextAccessor,
    UserContext userContext,
    ILogger<UserContextCircuitHandler> logger) : CircuitHandler
{
    public override async Task OnCircuitOpenedAsync(
        Circuit circuit, CancellationToken cancellationToken)
    {
        string? sub = null;

        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                sub = authState.User.FindFirst("sub")?.Value
                    ?? authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(ex, "Error getting authentication state from AuthenticationStateProvider");
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (string.IsNullOrEmpty(sub) && httpContext is not null)
        {
            var authentication = await httpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            if (authentication.Succeeded && authentication.Principal?.Identity?.IsAuthenticated == true)
            {
                sub = authentication.Principal.FindFirst("sub")?.Value
                    ?? authentication.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        if (string.IsNullOrEmpty(sub))
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Cannot initialize circuit user session: request or circuit is not authenticated.");

            userContext.MarkSessionExpired();
            return;
        }

        var sessionTokens = sessionStore.Get(sub);
        if (sessionTokens is not null)
        {
            userContext.Initialize(
                sub,
                sessionTokens.AccessToken,
                sessionTokens.RefreshToken,
                sessionTokens.ExpiresAt);
            return;
        }

        if (httpContext is not null)
        {
            var authentication = await httpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            if (authentication.Succeeded)
            {
                var properties = authentication.Properties;
                var accessToken = properties?.GetTokenValue("access_token");
                var refreshToken = properties?.GetTokenValue("refresh_token");
                var expiresAt = TryParseAccessTokenExpiresAt(properties?.GetTokenValue("expires_at"));

                if (!string.IsNullOrEmpty(accessToken))
                {
                    sessionStore.Store(sub, accessToken, refreshToken, expiresAt);
                    userContext.Initialize(sub, accessToken, refreshToken, expiresAt);
                    return;
                }
            }
        }

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("Authenticated user {Sub} has no access token in session store or cookie properties.", sub);
        }

        userContext.Initialize(sub, null, null, null);
    }

    private static DateTimeOffset? TryParseAccessTokenExpiresAt(string? expiresAt)
        => DateTimeOffset.TryParse(expiresAt, out var parsed) ? parsed : null;
}