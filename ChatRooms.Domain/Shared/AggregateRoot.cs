namespace ChatRooms.Domain.Shared;

public abstract class AggregateRoot<TId>(TId id, DateTime dateTime) : Entity<TId>(id, dateTime) where TId : struct, IEquatable<TId>
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
