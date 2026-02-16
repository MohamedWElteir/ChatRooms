using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Domain.Shared;

public abstract class AggregateRoot<TId> : Entity<TId>, ISoftDeletable where TId : struct, IEquatable<TId>
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = [];
    public int Version { get; private set; } = 0;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _uncommittedDomainEvents;

    public bool IsDeleted => DeletedAt.HasValue;
    public DateTimeUtc? DeletedAt { get; internal set; }
    public DeletionReason? Reason { get; internal set; }

    protected void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _uncommittedDomainEvents.Add(@event);
    }

    public abstract void Apply(IDomainEvent @event);
    public void ClearDomainEvents() => _uncommittedDomainEvents.Clear();
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var @event in history)
        {
            Apply(@event);
            Version++;
        }
    }
}
