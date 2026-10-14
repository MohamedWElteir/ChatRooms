using System.Collections.Concurrent;

namespace ChatRooms.Blazor.HttpClients;

/// <summary>
/// Thread-safe in-memory store holding active user session tokens keyed by user subject identifier (sub).
/// Bridges OpenID Connect token responses and SignalR circuits without exposing tokens in client HTML markup.
/// </summary>
public sealed class ServerSessionStore
{
    private readonly ConcurrentDictionary<string, UserSessionTokens> _sessions = new();

    public void Store(string sub, string accessToken, string? refreshToken, DateTimeOffset? expiresAt)
    {
        _sessions[sub] = new UserSessionTokens(accessToken, refreshToken, expiresAt);
    }

    public UserSessionTokens? Get(string sub)
    {
        return _sessions.TryGetValue(sub, out var tokens) ? tokens : null;
    }

    public void Remove(string sub)
    {
        _sessions.TryRemove(sub, out _);
    }
}

public sealed record UserSessionTokens(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt);
