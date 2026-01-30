using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public RoomCode Code { get; private set; }
    public RoomStatus Status { get; private set; }
    public int CurrentParticipantsCount { get; private set; } = 0;

    private Room(RoomId id, DateTimeUtc createdAt) : base(id, createdAt) { }
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
            case RoomRestoredDomainEvent e:
                Apply(e);
                break;

            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(Room)}");
        }
    }

    public static Room Create(Name name, Capacity capacity, DateTimeUtc occurredAt)
    {
        var room = new Room(RoomId.New(), occurredAt);
        room.Raise(new RoomCreatedDomainEvent(room.Id, name, RoomCode.New(), capacity, occurredAt));
        return room;

    }

    public void Join(DateTimeUtc occurredAt)
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be joined.");
        if (CurrentParticipantsCount >= Capacity.Value)
            throw new InvalidOperationException("Room capacity reached.");

        Raise(new RoomParticipantJoinedDomainEvent(occurredAt));
    }
    public void Leave(DateTimeUtc occurredAt)
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be left.");
        if (CurrentParticipantsCount <= 0)
            throw new InvalidOperationException("No participants to leave.");
        Raise(new RoomParticipantLeftDomainEvent(occurredAt));
    }
    public void Rename(Name newName, DateTimeUtc occurredAt)
    {
        if (Name == newName)
            return;
        Raise(new RoomRenamedDomainEvent(Id, newName, occurredAt));
    }
    public void Archive(DateTimeUtc occurredAt)
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be archived.");

        Raise(new RoomArchivedDomainEvent(Id, occurredAt));
    }

    public void Delete(DateTimeUtc occurredAt, DeletionReason reason)
    {
        if (Status == RoomStatus.Deleted)
            throw new InvalidOperationException("Can't delete a deleted room.");

        if (Status == RoomStatus.Active && reason == Enums.DeletionReason.Inactivity)
            throw new InvalidOperationException("Active rooms cannot be deleted due to inactivity.");

        Raise(new RoomDeletedDomainEvent(Id, reason, occurredAt));
    }

    public void ChangeCapacity(Capacity newCapacity, DateTimeUtc occurredAt)
    {
        if (Capacity == newCapacity)
            return;
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can change capacity.");
        if (newCapacity.Value < CurrentParticipantsCount)
            throw new InvalidOperationException("New capacity cannot be less than current participants count.");

        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity, occurredAt));
    }

    public void Restore(DateTimeUtc occurredAt)
    {
        if (Status != RoomStatus.Archived)
            throw new InvalidOperationException("Only archived rooms can be restored.");
        Raise(new RoomRestoredDomainEvent(occurredAt));
    }

    #region Event Appliers
    private void Apply(RoomCreatedDomainEvent @event)
    {
        if (Status != default)
            throw new InvalidOperationException("Room already created.");

        Id = @event.RoomId;
        Name = @event.Name;
        Capacity = @event.Capacity;
        Code = @event.Code;
        Status = RoomStatus.Active;
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
        DeletionReason = @event.DeletionReason;
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
    private void Apply(RoomRestoredDomainEvent @event)
    {
        Status = RoomStatus.Active;
        UpdatedAt = @event.OccurredAt;
    }
    #endregion
}
