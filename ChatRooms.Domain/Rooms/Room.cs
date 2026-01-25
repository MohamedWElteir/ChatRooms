using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : Entity<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public string RoomCode => Id.Value.ToString()[..8].ToUpperInvariant();
    public string RoomLink => $"https://chatrooms.example.com/rooms/{RoomCode}";
    private Room(RoomId id, Name name, Capacity capacity, DateTime createdAt) : base(id, createdAt)
    {
        Name = name;
        Capacity = capacity;
        Raise(new RoomCreatedDomainEvent(id, name, capacity, createdAt));

    }

    public static Room Create(Name name, Capacity capacity, DateTime createdAt)
    {
        var room = new Room(RoomId.New(), name, capacity, createdAt);

        return room;

    }
}
