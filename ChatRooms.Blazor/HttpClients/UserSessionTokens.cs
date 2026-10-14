namespace ChatRooms.Blazor.HttpClients;

public sealed record UserSessionTokens(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt);
