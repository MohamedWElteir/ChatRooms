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

var clientSecret = config["Keycloak:ClientSecret"]
    ?? throw new InvalidOperationException("Keycloak:ClientSecret is not configured.");

var secretResponse = await http.PutAsJsonAsync(
    $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{clientInternalId}",
    new { secret = clientSecret, clientAuthenticatorType = "client-secret" });

secretResponse.EnsureSuccessStatusCode();

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("BFF client secret updated successfully.");

var realmMgmtClients = await http.GetFromJsonAsync<JsonElement[]>(
    $"{keycloakBaseUrl}/admin/realms/{realm}/clients?clientId=realm-management");

if (realmMgmtClients is not null && realmMgmtClients.Length > 0)
{
    var realmMgmtClientId = realmMgmtClients[0].GetProperty("id").GetString()!;

    if (logger.IsEnabled(LogLevel.Information))
        logger.LogInformation("Found realm-management client ID: {Id}", realmMgmtClientId);

    var rolesResponse = await http.GetFromJsonAsync<JsonElement[]>(
        $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{realmMgmtClientId}/roles");

    string? manageUsersRoleId = null;
    if (rolesResponse is not null)
    {
        foreach (var role in rolesResponse)
        {
            if (role.TryGetProperty("name", out var nameProp) && nameProp.GetString() == "manage-users")
            {
                manageUsersRoleId = role.GetProperty("id").GetString();
                break;
            }
        }
    }

    if (manageUsersRoleId is not null)
    {
        var serviceAccountUser = await http.GetFromJsonAsync<JsonElement>(
            $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{clientInternalId}/service-account-user");

        var serviceAccountUserId = serviceAccountUser.GetProperty("id").GetString()!;

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Found service account user ID: {Id}", serviceAccountUserId);

        var roleMappingPayload = new[]
        {
            new { id = manageUsersRoleId, name = "manage-users" }
        };

        var roleMappingResponse = await http.PostAsJsonAsync(
            $"{keycloakBaseUrl}/admin/realms/{realm}/users/{serviceAccountUserId}/role-mappings/clients/{realmMgmtClientId}",
            roleMappingPayload);

        if (roleMappingResponse.IsSuccessStatusCode)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Granted manage-users role to chatrooms-bff service account.");
        }
        else
        {
            var error = await roleMappingResponse.Content.ReadAsStringAsync();
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Failed to grant manage-users role: {Error}", error);
        }
    }
    else
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("manage-users role not found in realm-management client.");
    }
}
else
{
    if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning("realm-management client not found.");
}

static Dictionary<string, object> CloneAsDictionary(JsonElement element)
{
    var dict = new Dictionary<string, object>();
    foreach (var prop in element.EnumerateObject())
    {
        dict[prop.Name] = ConvertJsonValue(prop.Value);
    }
    return dict;
}

static object[] CloneAsArray(JsonElement element)
{
    return element.EnumerateArray().Select(ConvertJsonValue).ToArray();
}

static object ConvertJsonValue(JsonElement value)
{
    return value.ValueKind switch
    {
        JsonValueKind.String => value.GetString()!,
        JsonValueKind.Number when value.TryGetInt32(out var i) => i,
        JsonValueKind.Number => value.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => string.Empty,
        JsonValueKind.Object => CloneAsDictionary(value),
        JsonValueKind.Array => CloneAsArray(value),
        _ => value.GetRawText()
    };
}

var userProfileComponents = await http.GetFromJsonAsync<JsonElement[]>(
    $"{keycloakBaseUrl}/admin/realms/{realm}/components?parent=org.keycloak.userprofile.UserProfileProvider");

if (userProfileComponents is not null && userProfileComponents.Length > 0)
{
    var component = userProfileComponents[0];
    var componentId = component.GetProperty("id").GetString()!;
    var componentConfig = component.GetProperty("config").GetProperty("kc.user.profile.config")[0].GetString()!;

    if (logger.IsEnabled(LogLevel.Information))
        logger.LogInformation("Found user profile component: {Id}", componentId);

    var profileConfig = JsonDocument.Parse(componentConfig).RootElement;
    var attributes = profileConfig.GetProperty("attributes").EnumerateArray().ToList();

    var updatedAttributes = new List<Dictionary<string, object>>();
    foreach (var attr in attributes)
    {
        var attrName = attr.GetProperty("name").GetString()!;
        var attrDict = new Dictionary<string, object>();

        foreach (var prop in attr.EnumerateObject())
        {
            if (prop.Name == "required" && attrName == "systemuserid")
                continue;

            attrDict[prop.Name] = ConvertJsonValue(prop.Value);
        }

        updatedAttributes.Add(attrDict);
    }

    var newProfileConfig = new Dictionary<string, object>
    {
        ["attributes"] = updatedAttributes.ToArray()
    };

    var newProfileJson = JsonSerializer.Serialize(newProfileConfig, new JsonSerializerOptions { WriteIndented = false });

    var updateComponent = new Dictionary<string, object>
    {
        ["id"] = componentId,
        ["providerId"] = "declarative-user-profile",
        ["providerType"] = "org.keycloak.userprofile.UserProfileProvider",
        ["config"] = new Dictionary<string, object[]>
        {
            ["kc.user.profile.config"] = [newProfileJson]
        }
    };

    var updateJson = JsonSerializer.Serialize(updateComponent, new JsonSerializerOptions { WriteIndented = false });

    var userProfileResponse = await http.PutAsync(
        $"{keycloakBaseUrl}/admin/realms/{realm}/components/{componentId}",
        new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json"));

    if (userProfileResponse.IsSuccessStatusCode)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("User profile updated: systemuserid is now optional.");
    }
    else
    {
        var error = await userProfileResponse.Content.ReadAsStringAsync();
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("Failed to update user profile: {Error}", error);
    }
}
else
{
    if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning("User profile component not found.");
}


await app.StopAsync();