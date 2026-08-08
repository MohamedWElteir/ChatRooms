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

var blazorUrlHttps = config["services:chatrooms-blazor:https:0"];
var blazorUrlHttp = config["services:chatrooms-blazor:http:0"];
if (blazorUrlHttps is null && blazorUrlHttp is null)
    throw new InvalidOperationException("Blazor service URL not found.");

var blazorUrls = new[] { blazorUrlHttps, blazorUrlHttp }
    .Where(u => u is not null)
    .Select(u => u!.TrimEnd('/'))
    .Distinct()
    .ToArray();

var adminUser = config["Keycloak:AdminUser"] ?? "admin";
var adminPass = config["Keycloak:AdminPassword"] ?? "admin";
var realm = config["Keycloak:Realm"] ?? "chatrooms";
var bffClientId = config["Keycloak:BffClientId"] ?? "chatrooms-bff";

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("KeycloakSetup starting. Keycloak: {Kc}, BFF: {Bff}", keycloakBaseUrl, bffBaseUrl);

var httpFactory = app.Services.GetRequiredService<IHttpClientFactory>();
using var http = httpFactory.CreateClient();

string? adminToken = null;
for (var attempt = 1; attempt <= 10; attempt++)
{
    using var tokenResponse = await http.PostAsync(
        $"{keycloakBaseUrl}/realms/master/protocol/openid-connect/token",
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = adminUser,
            ["password"] = adminPass
        }), app.Lifetime.ApplicationStopping);

    if (tokenResponse.IsSuccessStatusCode)
    {
        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(
            app.Lifetime.ApplicationStopping);
        adminToken = tokenJson.GetProperty("access_token").GetString();
        break;
    }

    if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning("Admin token attempt {Attempt}/10 failed with status {Status}.",
            attempt, tokenResponse.StatusCode);

    if (attempt < 10)
        await Task.Delay(TimeSpan.FromSeconds(3), app.Lifetime.ApplicationStopping);
}

if (adminToken is null)
    throw new InvalidOperationException("Unable to acquire a Keycloak admin token after 10 attempts.");

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

var redirectUris = new[] { $"{bffBaseUrl}/signin-oidc" }
    .Concat(blazorUrls.Select(url => $"{url}/signin-oidc"))
    .Distinct()
    .ToArray();

var logoutRedirectUris = new[] { $"{bffBaseUrl}/signout-callback-oidc" }
    .Concat(blazorUrls.Select(url => $"{url}/signout-callback-oidc"))
    .Distinct()
    .ToArray();

var patch = new
{
    redirectUris,
    webOrigins = blazorUrls.Append(bffBaseUrl).Distinct().ToArray(),
    attributes = new Dictionary<string, string>
    {
        ["post.logout.redirect.uris"] = string.Join("##", logoutRedirectUris)
    }
};

using var patchResponse = await http.PutAsJsonAsync(
    $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{clientInternalId}",
    patch);

patchResponse.EnsureSuccessStatusCode();

if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("BFF client updated. RedirectURIs now point to {Bff}", bffBaseUrl);

var existingMappers = await http.GetFromJsonAsync<JsonElement[]>(
    $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{clientInternalId}/protocol-mappers/models");

var hasApiAudienceMapper = existingMappers is not null
    && existingMappers.Any(m => m.TryGetProperty("name", out var n) && n.GetString() == "api-audience-mapper");

if (!hasApiAudienceMapper)
{
    var audienceMapperPayload = new
    {
        name = "api-audience-mapper",
        protocol = "openid-connect",
        protocolMapper = "oidc-audience-mapper",
        consentRequired = false,
        config = new Dictionary<string, string>
        {
            ["access.token.claim"] = "true",
            ["id.token.claim"] = "true",
            ["userinfo.token.claim"] = "true",
            ["included.client.audience"] = "chatrooms-api"
        }
    };

    using var mapperResponse = await http.PostAsJsonAsync(
        $"{keycloakBaseUrl}/admin/realms/{realm}/clients/{clientInternalId}/protocol-mappers/models",
        audienceMapperPayload);

    if (mapperResponse.IsSuccessStatusCode)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Added api-audience-mapper to chatrooms-bff client.");
    }
    else
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("Failed to add api-audience-mapper with status {Status}.",
                mapperResponse.StatusCode);
    }
}
else
{
    if (logger.IsEnabled(LogLevel.Information))
        logger.LogInformation("api-audience-mapper already exists on chatrooms-bff client.");
}

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

        using var roleMappingResponse = await http.PostAsJsonAsync(
            $"{keycloakBaseUrl}/admin/realms/{realm}/users/{serviceAccountUserId}/role-mappings/clients/{realmMgmtClientId}",
            roleMappingPayload);

        if (roleMappingResponse.IsSuccessStatusCode)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Granted manage-users role to chatrooms-bff service account.");
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("Failed to grant manage-users role with status {Status}.",
                    roleMappingResponse.StatusCode);
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

    using var userProfileResponse = await http.PutAsync(
        $"{keycloakBaseUrl}/admin/realms/{realm}/components/{componentId}",
        new StringContent(updateJson, System.Text.Encoding.UTF8, "application/json"));

    if (userProfileResponse.IsSuccessStatusCode)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("User profile updated: systemuserid is now optional.");
    }
    else
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("Failed to update user profile with status {Status}.",
                userProfileResponse.StatusCode);
    }
}
else
{
    if (logger.IsEnabled(LogLevel.Warning))
        logger.LogWarning("User profile component not found.");
}


await app.StopAsync();
