namespace ChatRooms.BFF.Services;

public interface IKeycloakAdminService
{
    Task<string> CreateUserAsync(
        RegisterBffRequest request,
        string adminToken,
        CancellationToken ct = default);

    Task SetUserAttributeAsync(
        string keycloakUserId,
        string attributeName,
        string attributeValue,
        string adminToken,
        CancellationToken ct = default);

    Task DeleteUserAsync(
        string keycloakUserId,
        string adminToken,
        CancellationToken ct = default);
}