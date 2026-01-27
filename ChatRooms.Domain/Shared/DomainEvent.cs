using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Domain.Shared;

public abstract record DomainEvent(DateTime EventOccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTime OccurredAt => EventOccurredAt;
}
