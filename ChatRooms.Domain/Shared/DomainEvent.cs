using ChatRooms.Domain.Shared.Contracts;
namespace ChatRooms.Domain.Shared;

public abstract record DomainEvent : IDomainEvent
{    
    public Guid Id => Guid.NewGuid();
    public DateTimeUtc OccurredAt => DateTimeUtc.NowUtc();
    public DateTimeKind Kind => DateTimeKind.Utc;
}
