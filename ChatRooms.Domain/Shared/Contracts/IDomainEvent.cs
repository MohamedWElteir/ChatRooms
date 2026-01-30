namespace ChatRooms.Domain.Shared.Contracts;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeUtc OccurredAt { get; }
}
