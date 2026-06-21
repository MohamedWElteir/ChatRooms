using System.Text.Json;

namespace ChatRooms.BFF.Services;

public sealed class KeycloakTokenService(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<KeycloakTokenService> logger)
{
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetServiceAccountTokenAsync(CancellationToken ct = default)
    {
        if (DateTimeOffset.UtcNow < _tokenExpiry && _cachedToken is not null)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (DateTimeOffset.UtcNow < _tokenExpiry && _cachedToken is not null)
                return _cachedToken;

            var realm = config["Keycloak:Realm"] ?? "chatrooms";
            var clientId = config["Keycloak:ClientId"] ?? "chatrooms-bff";
            var secret = config["Keycloak:ClientSecret"]
                ?? throw new InvalidOperationException("Keycloak client secret not configured.");

            logger.LogDebug("Refreshing service account token for {ClientId}", clientId);

            var response = await httpClient.PostAsync(
                $"/realms/{realm}/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = secret
                }), ct);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            _cachedToken = json.GetProperty("access_token").GetString()!;
            var expiresIn = json.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}