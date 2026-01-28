namespace ChatRooms.Domain.Shared.Contracts;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
}
