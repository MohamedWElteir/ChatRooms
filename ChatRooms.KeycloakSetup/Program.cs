using ChatRooms.ServiceDefaults;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var config = app.Configuration;


var keycloakBaseUrl = config["services:keycloak:http:0"]
    ?? throw new InvalidOperationException("Keycloak service URL not found.");

var bffBaseUrl = config["services:chatrooms-bff:http:0"]
    ?? throw new InvalidOperationException("BFF service URL not found.");

var adminUser = config["Keycloak:AdminUser"] ?? "admin";
var adminPass = config["Keycloak:AdminPassword"] ?? "admin";
var realm = config["Keycloak:Realm"] ?? "chatrooms";
var bffClientId = config["Keycloak:BffClientId"] ?? "chatrooms-bff";

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("KeycloakSetup starting. Keycloak: {Kc}, BFF: {Bff}", keycloakBaseUrl, bffBaseUrl);

var httpFactory = app.Services.GetRequiredService<IHttpClientFactory>();
using var http = httpFactory.CreateClient();

var tokenResponse = await http.PostAsync(
    $"{keycloakBaseUrl}/realms/master/protocol/openid-connect/token",
    new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "password",
        ["client_id"] = "admin-cli",
        ["username"] = adminUser,
        ["password"] = adminPass
    }));

tokenResponse.EnsureSuccessStatusCode();
var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
var adminToken = tokenJson.GetProperty("access_token").GetString()!;

http.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("Admin token acquired.");

var clientsResponse = await http.GetFromJsonAsync<JsonElement[]>(
    $"{keycloakBaseUrl}/admin/realms/{realm}/clients?clientId={bffClientId}");

if (clientsResponse is null || clientsResponse.Length == 0)
    throw new InvalidOperationException($"Client '{bffClientId}' not found in realm '{realm}'.");

var clientInternalId = clientsResponse[0].GetProperty("id").GetString()!;

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("Found BFF client internal ID: {Id}", clientInternalId);

var patch = new
{
    redirectUris = new[]
    {
        $"{bffBaseUrl}/signin-oidc",
        $"{bffBaseUrl}/*"
    },
    webOrigins = new[] { bffBaseUrl },
    attributes = new Dictionary<string, string>
    {
        ["post.logout.redirect.uris"] = $"{bffBaseUrl}/signout-callback-oidc##{bffBaseUrl}/*"
    }
};

var patchResponse = await http.PutAsJsonAsync(
    $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{clientInternalId}",
    patch);

patchResponse.EnsureSuccessStatusCode();

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("BFF client updated. RedirectURIs now point to {Bff}", bffBaseUrl);


await app.StopAsync();