namespace ChatRooms.Blazor.HttpClients;

public sealed class UserContext
{
    public string? AccessToken { get; private set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; private set; }

    public bool HasValidAccessToken =>
        AccessToken is not null &&
        (AccessTokenExpiresAt is null || AccessTokenExpiresAt > DateTimeOffset.UtcNow);

    public void SetAccessToken(string? accessToken, string? expiresAt)
    {
        AccessToken = accessToken;
        AccessTokenExpiresAt = DateTimeOffset.TryParse(expiresAt, out var parsedExpiresAt)
            ? parsedExpiresAt
            : null;
    }
}
