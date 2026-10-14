using System.Net.Http.Headers;

namespace ChatRooms.Blazor.HttpClients;

/// <summary>
/// Attaches the circuit user's bearer token to outgoing API calls. Circuit-scoped services
/// are resolved through <see cref="CircuitServicesAccessor"/> because IHttpClientFactory
/// creates this handler in its own DI scope, where <see cref="UserContext"/> would be empty.
/// </summary>
public sealed class AuthorizationDelegatingHandler(
    CircuitServicesAccessor circuitServicesAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var services = circuitServicesAccessor.Services
            ?? throw new UnauthorizedAccessException(
                "No active Blazor circuit is available to resolve the user session.");

        var userContext = services.GetRequiredService<UserContext>();
        var accessTokenRefresher = services.GetRequiredService<AccessTokenRefresher>();

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