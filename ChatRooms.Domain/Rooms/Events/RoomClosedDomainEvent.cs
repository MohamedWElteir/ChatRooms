using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events
{
    public sealed record RoomClosedDomainEvent(RoomId RoomId, DateTime ClosedAt) : DomainEvent(ClosedAt);
}