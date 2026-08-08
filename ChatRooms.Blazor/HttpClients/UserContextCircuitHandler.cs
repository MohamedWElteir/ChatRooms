using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Security.Claims;

namespace ChatRooms.Blazor.HttpClients;

public sealed class UserContextCircuitHandler(
    IHttpContextAccessor httpContextAccessor,
    UserContext userContext,
    ILogger<UserContextCircuitHandler> logger) : CircuitHandler
{
    public override async Task OnCircuitOpenedAsync(
        Circuit circuit, CancellationToken cancellationToken)
    {
        // The access/refresh tokens live in the authentication cookie's
        // AuthenticationProperties (SaveTokens). They are only readable while an
        // HTTP request is being processed, so circuit establishment — which runs
        // inside the initial connection request — is the last guaranteed point at
        // which they can be captured. After this point the circuit must not depend
        // on HttpContext (it is null for regular circuit activity). If that request
        // context is unavailable here, the circuit session fails explicitly instead
        // of silently degrading to anonymous API calls.
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning(
                    "Cannot initialize circuit user session: no HTTP request context available at circuit open.");

            userContext.MarkSessionExpired();
            return;
        }

        var authentication = await httpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authentication.Succeeded
            || authentication.Principal?.Identity?.IsAuthenticated != true)
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning(
                    "Cannot initialize circuit user session: request is not authenticated.");

            userContext.MarkSessionExpired();
            return;
        }

        var sub = authentication.Principal.FindFirst("sub")?.Value
            ?? authentication.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var properties = authentication.Properties;
        userContext.Initialize(
            sub,
            properties?.GetTokenValue("access_token"),
            properties?.GetTokenValue("refresh_token"),
            TryParseAccessTokenExpiresAt(properties?.GetTokenValue("expires_at")));
    }

    private static DateTimeOffset? TryParseAccessTokenExpiresAt(string? expiresAt)
        => DateTimeOffset.TryParse(expiresAt, out var parsed) ? parsed : null;
}