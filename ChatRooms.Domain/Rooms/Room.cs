using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public RoomCode Code { get; private set; }
    public RoomStatus Status { get; private set; } = RoomStatus.Active;
    private Room(RoomId id, Name name, Capacity capacity, DateTime createdAt) : base(id, createdAt)
    {
        Name = name;
        Capacity = capacity;
        Code = RoomCode.New();
        Raise(new RoomCreatedDomainEvent(id, name, capacity, createdAt));

    }

    public static Room Create(Name name, Capacity capacity, DateTime createdAt)
    {
        var room = new Room(RoomId.New(), name, capacity, createdAt);

        return room;

    }
    public void Rename(Name newName)
    {
        if (Name == newName)
            return;

        Name = newName;
        Raise(new RoomRenamedDomainEvent(Id, newName, DateTime.UtcNow));
    }
    public void Archive(DateTime archivedAt)
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be archived.");

        Status = RoomStatus.Archived;
        Raise(new RoomArchivedDomainEvent(Id, archivedAt));
    }

    public void Delete(DateTime deletedAt, DeleteCause reason)
    {
        if (Status == RoomStatus.Deleted)
            throw new InvalidOperationException("Can't delete a deleted room.");

        if (Status == RoomStatus.Active && reason == DeleteCause.Inactivity)
            throw new InvalidOperationException("Active rooms cannot be deleted due to inactivity.");

        Status = RoomStatus.Deleted;
        Raise(new RoomDeletedDomainEvent(Id, reason, deletedAt));
    }

    public void ChangeCapacity(Capacity newCapacity)
    {
        if (Capacity == newCapacity)
            return;
        // TODO: Add validation logic for decreasing capacity based on current number of participants

        Capacity = newCapacity;
        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity, DateTime.UtcNow));
    }


}
