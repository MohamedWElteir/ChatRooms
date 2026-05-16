using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Domain.Shared;

public abstract record DomainEvent(DateTimeUtc OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeKind Kind { get; } = DateTimeUtc.Kind;
    public int AggregateVersion { get; internal set; }
    public virtual bool Equals(DomainEvent? other) => other is not null && Id == other.Id;
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}