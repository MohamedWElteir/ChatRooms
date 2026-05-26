using System.Text.Json;

namespace ChatRooms.BFF.Services;

public sealed class KeycloakTokenService(
    IHttpClientFactory httpClientFactory,
    IConfiguration config)
{
    public async Task<string> GetServiceAccountTokenAsync(CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("keycloak-token");
        var realm = config["Keycloak:Realm"] ?? "chatrooms";
        var clientId = config["Keycloak:ClientId"] ?? "chatrooms-bff";
        var secret = config["Keycloak:ClientSecret"]
            ?? throw new InvalidOperationException("Keycloak client secret not configured.");

        var response = await http.PostAsync(
            $"/realms/{realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = secret
            }), ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("access_token").GetString()!;
    }
}