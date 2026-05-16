namespace ChatRooms.Domain.Shared;

public sealed record Error(string Code, string Message)
{
    public static implicit operator Error(string message) => new(string.Empty, message);
}
