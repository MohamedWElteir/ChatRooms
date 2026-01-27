using ChatRooms.Domain.Rooms.Events;

namespace ChatRooms.Domain.Rooms.Appliers;

internal static class RoomDeletedDomainEventApplier
{
    public static void Apply(Room room, RoomDeletedDomainEvent @event)
    {
        room.Status = Enums.RoomStatus.Deleted;
        room.DeletedAt = @event.DeletedAt;
        room.DeletionReason = @event.DeletionReason;
    }
}
