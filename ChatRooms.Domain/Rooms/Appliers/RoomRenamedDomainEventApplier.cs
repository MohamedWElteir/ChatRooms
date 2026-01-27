using ChatRooms.Domain.Rooms.Events;

namespace ChatRooms.Domain.Rooms.Appliers;

internal static class RoomRenamedDomainEventApplier
{
    public static void Apply(Room room, RoomRenamedDomainEvent @event)
    {
        room.Name = @event.NewName;
        room.UpdatedAt = @event.RenamedOn;
    }
}
