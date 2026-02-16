using ChatRooms.Domain.Shared.Contracts;
namespace ChatRooms.Domain.Shared;

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeUtc OccurredAt { get; } = DateTimeUtc.NowUtc();
    public DateTimeKind Kind { get; } = DateTimeKind.Utc;
    public virtual bool Equals(DomainEvent? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
