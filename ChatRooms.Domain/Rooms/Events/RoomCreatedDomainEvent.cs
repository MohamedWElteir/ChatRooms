using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomCreatedDomainEvent(RoomId RoomId, Name Name, RoomCode Code, Capacity Capacity) : DomainEvent;