namespace ChatRooms.Blazor.HttpClients;

internal sealed record RefreshedTokenResult(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);
