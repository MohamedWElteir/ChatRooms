using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Domain.Shared;

public abstract class AggregateRoot<TId>(TId id, DateTime dateTime) : Entity<TId>(id, dateTime) where TId : struct, IEquatable<TId>
{
    private readonly List<IDomainEvent> _uncommitedDomainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _uncommitedDomainEvents;

    protected void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _uncommitedDomainEvents.Add(@event);
    }

    public abstract void Apply(IDomainEvent @event);
    public void ClearDomainEvents() => _uncommitedDomainEvents.Clear();
}
