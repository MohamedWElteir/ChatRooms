using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public RoomCode Code { get; private set; }
    public RoomStatus Status { get; private set; } = RoomStatus.Active;


    private Room(RoomId id, DateTime createdAt) : base(id, createdAt) { }
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

            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(Room)}");
        }
    }

    public static Room Create(Name name, Capacity capacity, DateTime createdAt)
    {
        var room = new Room(RoomId.New(), createdAt);
        room.Raise(new RoomCreatedDomainEvent(room.Id, name, RoomCode.New(), capacity, createdAt));
        return room;

    }
    public void Rename(Name newName)
    {
        if (Name == newName)
            return;
        Raise(new RoomRenamedDomainEvent(Id, newName, DateTime.UtcNow));
    }
    public void Archive(DateTime archivedAt)
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be archived.");

        Raise(new RoomArchivedDomainEvent(Id, archivedAt));
    }

    public void Delete(DateTime deletedAt, DeletionReason reason)
    {
        if (Status == RoomStatus.Deleted)
            throw new InvalidOperationException("Can't delete a deleted room.");

        if (Status == RoomStatus.Active && reason == Enums.DeletionReason.Inactivity)
            throw new InvalidOperationException("Active rooms cannot be deleted due to inactivity.");

        Raise(new RoomDeletedDomainEvent(Id, reason, deletedAt));
    }

    public void ChangeCapacity(Capacity newCapacity)
    {
        if (Capacity == newCapacity)
            return;
        // TODO: Add validation logic for decreasing capacity based on current number of participants

        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity, DateTime.UtcNow));
    }

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
        DeletedAt = @event.DeletedAt;
        DeletionReason = @event.DeletionReason;
    }

    private void Apply(RoomRenamedDomainEvent @event)
    {
        Name = @event.NewName;
        UpdatedAt = @event.RenamedOn;
    }
}
