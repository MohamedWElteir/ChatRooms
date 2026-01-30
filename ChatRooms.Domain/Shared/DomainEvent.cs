using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Domain.Shared;

public abstract record DomainEvent(DateTimeUtc EventOccurredAt) : IDomainEvent
{    
    public Guid Id => Guid.NewGuid();
    public DateTimeUtc OccurredAt => EventOccurredAt;

}
