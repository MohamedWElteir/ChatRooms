namespace ChatRooms.Domain.Shared.Contracts;

public interface IAuditable
{
    DateTimeUtc CreatedAt { get; }
    DateTimeUtc? UpdatedAt { get; }
}
