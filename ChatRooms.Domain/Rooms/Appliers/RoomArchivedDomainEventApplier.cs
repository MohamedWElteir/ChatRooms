using ChatRooms.Domain.Rooms.Events;

namespace ChatRooms.Domain.Rooms.Appliers;

internal static class RoomArchivedDomainEventApplier
{
    public static void Apply(Room room, RoomArchivedDomainEvent @event)
    {
        room.Status = Enums.RoomStatus.Archived;
        room.UpdatedAt = @event.OccurredAt;
    }
}
