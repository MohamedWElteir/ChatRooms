namespace ChatRooms.BFF.Services;

public sealed class KeycloakAdminService(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<KeycloakAdminService> logger) : IKeycloakAdminService
{
    private readonly string _realm = config["Keycloak:Realm"] ?? "chatrooms";

    public async Task<string> CreateUserAsync(
        RegisterBffRequest request,
        string adminToken,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var payload = new
        {
            username = request.Name,
            email = request.Email,
            firstName = request.Name,
            lastName = "",
            enabled = true,
            emailVerified = false,
            credentials = new[]
            {
            new { type = "password", value = request.Password, temporary = false }
        }
        };

        var response = await httpClient.PostAsJsonAsync(
            $"/admin/realms/{_realm}/users", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "[DEBUG-b3f7] Keycloak user creation failed: {Status} {Error} BaseAddress={Base}",
                response.StatusCode, error, httpClient.BaseAddress);

            throw response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? new InvalidOperationException(
                    "An account with this email already exists.")
                : new InvalidOperationException(
                    $"Failed to create account: {response.StatusCode} - {error}");
        }

        var location = response.Headers.Location?.ToString()
            ?? throw new InvalidOperationException(
                "Keycloak did not return user location header.");

        return location.Split('/').Last();
    }

    public async Task SetUserAttributeAsync(
        string keycloakUserId,
        string attributeName,
        string attributeValue,
        string adminToken,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var payload = new
        {
            attributes = new Dictionary<string, string[]>
            {
                [attributeName] = [attributeValue]
            }
        };

        var response = await httpClient.PutAsJsonAsync(
            $"/admin/realms/{_realm}/users/{keycloakUserId}", payload, ct);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(
        string keycloakUserId,
        string adminToken,
        CancellationToken ct = default)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.DeleteAsync(
            $"/admin/realms/{_realm}/users/{keycloakUserId}", ct);

        if (!response.IsSuccessStatusCode)
            logger.LogWarning(
                "Failed to rollback Keycloak user {UserId}: {Status}",
                keycloakUserId, response.StatusCode);
    }
}