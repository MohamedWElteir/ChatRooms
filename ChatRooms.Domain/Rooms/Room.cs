using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Shared.Enums;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public RoomCode Code { get; private set; }
    public RoomStatus Status { get; private set; }
    public int CurrentParticipantsCount { get; private set; } = 0;

    private Room() : base() { }
    public override void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case RoomCreatedDomainEvent e:
                Apply(e);
                break;
            case RoomRenamedDomainEvent e:
                Apply(e);
                break;
            case RoomArchivedDomainEvent e:
                Apply(e);
                break;
            case RoomDeletedDomainEvent e:
                Apply(e);
                break;
            case RoomCapacityChangedDomainEvent e:
                Apply(e);
                break;
            case RoomParticipantJoinedDomainEvent e:
                Apply(e);
                break;
            case RoomParticipantLeftDomainEvent e:
                Apply(e);
                break;
            case RoomUnArchivedDomainEvent e:
                Apply(e);
                break;

            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(Room)}");
        }
    }

    public static Room Create(Name name, Capacity capacity, RoomCode roomCode, DateTimeUtc dateTime)
    {
        var room = new Room();
        if (!room.IsTransient())
            throw new InvalidOperationException("Only transient rooms can be created.");

        room.Raise(new RoomCreatedDomainEvent(RoomId.New(), name, roomCode, capacity, dateTime));
        return room;

    }

    public void Join(DateTimeUtc occurredAt)
    {
        EnsureActive();
        if (CurrentParticipantsCount >= Capacity.Value)
            throw new InvalidOperationException("Room capacity reached.");

        Raise(new RoomParticipantJoinedDomainEvent(Id, occurredAt));
    }
    public void Leave(DateTimeUtc occurredAt)
    {
        EnsureActive();
        if (CurrentParticipantsCount <= 0)
            throw new InvalidOperationException("No participants to leave.");
        Raise(new RoomParticipantLeftDomainEvent(Id, occurredAt));
    }
    public void Rename(Name newName, DateTimeUtc occurredAt)
    {
        EnsureActive();
        if (Name == newName)
            return;
        Raise(new RoomRenamedDomainEvent(Id, newName, occurredAt));
    }
    public void Archive(DateTimeUtc occurredAt)
    {
        if (Status == RoomStatus.Archived)
            return;
        EnsureActive();
        Raise(new RoomArchivedDomainEvent(Id, occurredAt));
    }

    public void Delete(DeletionReason reason, DateTimeUtc occurredAt)
    {
        EnsureNotDeleted();
        if (Status == RoomStatus.Active && reason == DeletionReason.Inactivity)
            throw new InvalidOperationException("Active rooms cannot be deleted due to inactivity.");

        Raise(new RoomDeletedDomainEvent(Id, reason, occurredAt));
    }

    public void ChangeCapacity(Capacity newCapacity, DateTimeUtc occurredAt)
    {
        EnsureActive();
        if (Capacity == newCapacity)
            return;
        if (newCapacity.Value < CurrentParticipantsCount)
            throw new InvalidOperationException("New capacity cannot be less than current participants count.");

        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity, occurredAt));
    }

    public void Restore(DateTimeUtc occurredAt)
    {
        if (Status != RoomStatus.Archived)
            throw new InvalidOperationException("Only archived rooms can be restored.");
        Raise(new RoomUnArchivedDomainEvent(Id, occurredAt));
    }

    #region Event Appliers
    private void Apply(RoomCreatedDomainEvent @event)
    {
        Id = @event.RoomId;
        Name = @event.Name;
        Capacity = @event.Capacity;
        Code = @event.Code;
        Status = RoomStatus.Active;
        CreatedAt = @event.OccurredAt;
    }

    private void Apply(RoomArchivedDomainEvent @event)
    {
        Status = RoomStatus.Archived;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(RoomCapacityChangedDomainEvent @event)
    {
        Capacity = @event.NewCapacity;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(RoomDeletedDomainEvent @event)
    {
        Status = RoomStatus.Deleted;
        DeletedAt = @event.OccurredAt;
        Reason = @event.DeletionReason;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(RoomRenamedDomainEvent @event)
    {
        Name = @event.NewName;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(RoomParticipantJoinedDomainEvent @event)
    {
        CurrentParticipantsCount++;
        UpdatedAt = @event.OccurredAt;
    }
    private void Apply(RoomParticipantLeftDomainEvent @event)
    {
        CurrentParticipantsCount--;
        UpdatedAt = @event.OccurredAt;
    }
    private void Apply(RoomUnArchivedDomainEvent @event)
    {
        Status = RoomStatus.Active;
        UpdatedAt = @event.OccurredAt;
    }
    #endregion

    #region Guard Clauses
    private void EnsureNotDeleted()
    {
        if (Status == RoomStatus.Deleted)
            throw new InvalidOperationException("Operation not allowed on deleted room.");
    }
    private void EnsureActive()
    {
        EnsureNotDeleted();
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Operation only allowed on active rooms.");
    }
    #endregion
}
