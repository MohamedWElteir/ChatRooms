namespace ChatRooms.Blazor.HttpClients;

public sealed class UserContext
{
    public string? Sub { get; private set; }

    public string? AccessToken { get; private set; }

    public DateTimeOffset? AccessTokenExpiresAt { get; private set; }

    public string? RefreshToken { get; private set; }

    public bool HasValidAccessToken =>
        AccessToken is not null &&
        (AccessTokenExpiresAt is null || AccessTokenExpiresAt > DateTimeOffset.UtcNow);

    public void Initialize(string? sub, string? accessToken, string? refreshToken, DateTimeOffset? accessTokenExpiresAt)
    {
        Sub = sub;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpiresAt = accessTokenExpiresAt;
    }

    public void MarkSessionExpired()
    {
        Sub = null;
        AccessToken = null;
        RefreshToken = null;
        AccessTokenExpiresAt = null;
    }
}