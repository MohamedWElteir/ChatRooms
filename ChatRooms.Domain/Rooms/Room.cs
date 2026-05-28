using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Shared.Errors;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public RoomCode Code { get; private set; }
    public RoomStatus Status { get; private set; }
    public int CurrentParticipantsCount { get; private set; }

    private Room() { }
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

    public static Result<Room> Create(Name name, Capacity capacity, RoomCode roomCode, DateTimeUtc dateTime)
    {
        var room = new Room();
        if (!room.IsTransient())
            return RoomErrors.NotTransient;

        room.Raise(new RoomCreatedDomainEvent(RoomId.New(), name, roomCode, capacity, room.CurrentParticipantsCount, dateTime));
        return room;

    }

    public Result Join(DateTimeUtc occurredAt)
    {
        var check = EnsureActive();
        if (check.IsFailure) return check;

        if (CurrentParticipantsCount >= Capacity.Value)
            return RoomErrors.CapacityReached;

        Raise(new RoomParticipantJoinedDomainEvent(Id, occurredAt));
        return Result.Success();
    }
    public Result Leave(DateTimeUtc occurredAt)
    {
        var check = EnsureActive();
        if (check.IsFailure) return check;

        if (CurrentParticipantsCount <= 0)
            return RoomErrors.NoParticipantsToLeave;

        Raise(new RoomParticipantLeftDomainEvent(Id, occurredAt));
        return Result.Success();
    }
    public Result Rename(Name newName, DateTimeUtc occurredAt)
    {
        var check = EnsureActive();
        if (check.IsFailure) return check;

        if (Name == newName)
            return Result.Success();

        Raise(new RoomRenamedDomainEvent(Id, newName, occurredAt));
        return Result.Success();
    }
    public Result Archive(DateTimeUtc occurredAt)
    {
        if (Status is RoomStatus.Archived)
            return Result.Success();

        var check = EnsureActive();
        if (check.IsFailure) return check;

        Raise(new RoomArchivedDomainEvent(Id, occurredAt));
        return Result.Success();
    }

    public Result Delete(DeletionReason reason, DateTimeUtc occurredAt)
    {
        var check = EnsureNotDeleted();
        if (check.IsFailure) return check;

        if (Status == RoomStatus.Active && reason == DeletionReason.Inactivity)
            return RoomErrors.ActiveRoomCannotBeDeletedDueToInactivity;

        Raise(new RoomDeletedDomainEvent(Id, reason, occurredAt));
        return Result.Success();
    }

    public Result ChangeCapacity(Capacity newCapacity, DateTimeUtc occurredAt)
    {
        var check = EnsureActive();
        if (check.IsFailure) return check;

        if (Capacity == newCapacity)
            return Result.Success();

        if (newCapacity.Value < CurrentParticipantsCount)
            return RoomErrors.NewCapacityCannotBeLessThanCurrentParticipants;

        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity, occurredAt));
        return Result.Success();
    }

    public Result Restore(DateTimeUtc occurredAt)
    {
        if (Status is not RoomStatus.Archived)
            return RoomErrors.OnlyArchivedCanBeRestored;

        Raise(new RoomUnArchivedDomainEvent(Id, occurredAt));
        return Result.Success();
    }

    #region Event Appliers
    private void Apply(RoomCreatedDomainEvent @event)
    {
        Id = @event.RoomId;
        Name = @event.Name;
        Capacity = @event.Capacity;
        Code = @event.Code;
        CurrentParticipantsCount = @event.CurrentParticipantsCount;
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
    private Result EnsureNotDeleted()
    {
        if (Status is RoomStatus.Deleted || IsDeleted)
            return RoomErrors.Deleted;

        return Result.Success();
    }
    private Result EnsureActive()
    {
        var check = EnsureNotDeleted();
        if (check.IsFailure) return check;

        if (Status is not RoomStatus.Active)
            return RoomErrors.NotActive;

        return Result.Success();
    }
    #endregion
}
