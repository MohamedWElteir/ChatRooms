using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;

namespace ChatRooms.Domain.Rooms.Appliers;

internal static class RoomCreatedDomainEventApplier
{
    public static void Apply(Room room, RoomCreatedDomainEvent @event)
    {
        room.Id = @event.RoomId;
        room.Name = @event.Name;
        room.Capacity = @event.Capacity;
        room.Code = RoomCode.New();
        room.Status = RoomStatus.Active;
    }
}
