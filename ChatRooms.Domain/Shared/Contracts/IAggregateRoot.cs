namespace ChatRooms.Domain.Shared.Contracts;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    int Version { get; }

    void Apply(IDomainEvent @event);
    void ClearDomainEvents();
}