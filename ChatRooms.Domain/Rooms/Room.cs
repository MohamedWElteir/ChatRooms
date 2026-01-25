using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : Entity<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public RoomCode RoomCode { get; private set; }
    private Room(RoomId id, Name name, Capacity capacity, DateTime createdAt) : base(id, createdAt)
    {
        Name = name;
        Capacity = capacity;
        RoomCode = RoomCode.New();
        Raise(new RoomCreatedDomainEvent(id, name, capacity, createdAt));

    }

    public static Room Create(Name name, Capacity capacity, DateTime createdAt)
    {
        var room = new Room(RoomId.New(), name, capacity, createdAt);

        return room;

    }
}
