namespace ChatRooms.Domain.Shared.Contracts;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
