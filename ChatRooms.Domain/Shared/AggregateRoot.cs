using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Domain.Shared;

public abstract class AggregateRoot<TId>(TId id, DateTimeUtc dateTime) : Entity<TId>(id, dateTime) where TId : struct, IEquatable<TId>
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = [];
    public int Version { get; private set; } = 0;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _uncommittedDomainEvents;

    protected void Raise(IDomainEvent @event)
    {
        Apply(@event);
        Version++;
        _uncommittedDomainEvents.Add(@event);
    }

    public abstract void Apply(IDomainEvent @event);
    public void ClearDomainEvents() => _uncommittedDomainEvents.Clear();
}
