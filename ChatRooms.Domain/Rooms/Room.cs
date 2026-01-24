using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
using ChatRooms.SharedKernel.Utils;
namespace ChatRooms.Domain.Rooms;

internal sealed class Room : Entity<RoomId>
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    private Room(RoomId id, Name name, Capacity capacity, IDateTimeProvider dateTimeProvider) : base(id, dateTimeProvider)
    {
        Name = name;
        Capacity = capacity;
        Raise(new RoomCreatedDomainEvent(id, name, capacity, dateTimeProvider.UtcNow));

    }

    public static Room Create(Name name, Capacity capacity, IDateTimeProvider dateTimeProvider)
    {
        var room = new Room(RoomId.New(), name, capacity, dateTimeProvider);

        return room;

    }
}
