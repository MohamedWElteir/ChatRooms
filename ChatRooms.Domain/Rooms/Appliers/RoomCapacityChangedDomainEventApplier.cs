using ChatRooms.Domain.Rooms.Events;

namespace ChatRooms.Domain.Rooms.Appliers;

internal static class RoomCapacityChangedDomainEventApplier
{
    public static void Apply(Room room, RoomCapacityChangedDomainEvent @event)
    {
        room.Capacity = @event.NewCapacity;
        room.UpdatedAt = @event.OccurredAt;
    }
}
