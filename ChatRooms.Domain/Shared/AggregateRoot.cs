using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Shared.Enums;
using System.Text.Json.Serialization;

namespace ChatRooms.Domain.Shared;

public abstract class AggregateRoot<TId> : Entity<TId>, ISoftDeletable, IAggregateRoot where TId : struct, IEquatable<TId>
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = [];
    [JsonIgnore]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _uncommittedDomainEvents;
    public int Version
    {
        get;
        private set
        {
            field = value >= 0
                ? value
                : throw new InvalidOperationException("Version cannot be negative.");
        }
    }
    public bool IsDeleted => DeletedAt.HasValue;
    public DateTimeUtc? DeletedAt { get; internal set; }
    public DeletionReason? Reason { get; internal set; }

    protected void Raise(IDomainEvent @event)
    {
        Apply(@event);
        Version++;
        if (@event is DomainEvent domainEvent)
            domainEvent.AggregateVersion = Version;

        _uncommittedDomainEvents.Add(@event);
    }

    public abstract void Apply(IDomainEvent @event);
    public void ClearDomainEvents() => _uncommittedDomainEvents.Clear();
}
