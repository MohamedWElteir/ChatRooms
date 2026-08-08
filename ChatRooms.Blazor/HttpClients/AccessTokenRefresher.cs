using System.Text.Json;

namespace ChatRooms.Blazor.HttpClients;

/// <summary>
/// Exchanges a Keycloak refresh token for a new access token using the standard
/// OIDC refresh-token grant against the same confidential client used for login
/// (<c>chatrooms-bff</c>). Scoped per circuit: it holds no cached state beyond the
/// single-flight guard so concurrent requests on one circuit share one exchange.
/// </summary>
public sealed class AccessTokenRefresher(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AccessTokenRefresher> logger)
{
    private const string KeycloakHttpClientName = "keycloak";
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Returns the user's valid access token, refreshing it exactly once when
    /// concurrent callers race. The <see cref="UserContext"/> is the source of
    /// truth: the refresh token is read from it only after the single-flight
    /// guard is acquired, and the result is written back before the guard is
    /// released, so no caller ever exchanges a stale or already-rotated token.
    /// </summary>
    public async Task<string?> GetValidAccessTokenAsync(
        UserContext userContext,
        CancellationToken cancellationToken)
    {
        if (userContext.HasValidAccessToken)
            return userContext.AccessToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (userContext.HasValidAccessToken)
                return userContext.AccessToken;

            var refreshToken = userContext.RefreshToken;
            if (refreshToken is null)
                return null;

            var refreshed = await ExchangeRefreshTokenAsync(
                refreshToken, cancellationToken);
            if (refreshed is null)
                return null;

            userContext.Initialize(
                userContext.Sub,
                refreshed.AccessToken,
                refreshed.RefreshToken ?? refreshToken,
                refreshed.AccessTokenExpiresAt);

            return refreshed.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<RefreshedTokenResult?> ExchangeRefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken)
    {
        var realm = configuration["Keycloak:Realm"] ?? "chatrooms";
        var clientId = configuration["Keycloak:ClientId"] ?? "chatrooms-bff";
        var clientSecret = configuration["Keycloak:ClientSecret"]
            ?? throw new InvalidOperationException("Keycloak client secret not configured.");

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Refreshing access token for {ClientId}", clientId);

        var httpClient = httpClientFactory.CreateClient(KeycloakHttpClientName);
        using var response = await httpClient.PostAsync(
            $"/realms/{realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["refresh_token"] = refreshToken
            }), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Access token refresh failed with status {Status}.", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var accessToken = json.GetProperty("access_token").GetString();
        if (accessToken is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Access token refresh succeeded but returned no access token.");
            return null;
        }

        var expiresIn = json.TryGetProperty("expires_in", out var expiresInElement)
            ? expiresInElement.GetInt32()
            : 300;

        return new RefreshedTokenResult(
            accessToken,
            json.TryGetProperty("refresh_token", out var refreshTokenElement)
                ? refreshTokenElement.GetString()
                : null,
            DateTimeOffset.UtcNow.AddSeconds(Math.Max(expiresIn - 30, 0)));
    }
}

internal sealed record RefreshedTokenResult(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);