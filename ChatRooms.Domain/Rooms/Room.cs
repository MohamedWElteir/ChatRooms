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
            case RoomRestoredDomainEvent e:
                Apply(e);
                break;

            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(Room)}");
        }
    }

    public static Room Create(Name name, Capacity capacity)
    {
        var room = new Room();
        if(!room.IsTransient())
            throw new InvalidOperationException("Only transient rooms can be created.");

        room.Raise(new RoomCreatedDomainEvent(RoomId.New(), name, RoomCode.New(), capacity));
        return room;

    }

    public void Join()
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be joined.");
        if (CurrentParticipantsCount >= Capacity.Value)
            throw new InvalidOperationException("Room capacity reached.");

        Raise(new RoomParticipantJoinedDomainEvent());
    }
    public void Leave()
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be left.");
        if (CurrentParticipantsCount <= 0)
            throw new InvalidOperationException("No participants to leave.");
        Raise(new RoomParticipantLeftDomainEvent());
    }
    public void Rename(Name newName)
    {
        if (Name == newName)
            return;
        Raise(new RoomRenamedDomainEvent(Id, newName));
    }
    public void Archive()
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be archived.");

        Raise(new RoomArchivedDomainEvent(Id));
    }

    public void Delete(DeletionReason reason)
    {
        if (Status == RoomStatus.Deleted)
            throw new InvalidOperationException("Can't delete a deleted room.");

        if (Status == RoomStatus.Active && reason == Enums.DeletionReason.Inactivity)
            throw new InvalidOperationException("Active rooms cannot be deleted due to inactivity.");

        Raise(new RoomDeletedDomainEvent(Id, reason));
    }

    public void ChangeCapacity(Capacity newCapacity)
    {
        if (Capacity == newCapacity)
            return;
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can change capacity.");
        if (newCapacity.Value < CurrentParticipantsCount)
            throw new InvalidOperationException("New capacity cannot be less than current participants count.");

        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity));
    }

    public void Restore()
    {
        if (Status != RoomStatus.Archived)
            throw new InvalidOperationException("Only archived rooms can be restored.");
        Raise(new RoomRestoredDomainEvent());
    }

    #region Event Appliers
    private void Apply(RoomCreatedDomainEvent @event)
    {
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
