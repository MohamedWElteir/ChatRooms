using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;
public sealed record RoomCapacityChangedDomainEvent(RoomId RoomId, Capacity NewCapacity, DateTime ChangeDatetime) : DomainEvent(ChangeDatetime);