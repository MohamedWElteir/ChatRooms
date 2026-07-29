using System.Collections.Concurrent;

namespace ChatRooms.Blazor.HttpClients;

public sealed class TokenStore
{
    private readonly ConcurrentDictionary<string, string> _tokens = new();

    public void Set(string userId, string token) => _tokens[userId] = token;

    public string? Get(string userId) =>
        _tokens.TryGetValue(userId, out var token) ? token : null;

    public void Remove(string userId) => _tokens.TryRemove(userId, out _);
}
