using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomRenamedDomainEvent(RoomId RoomId, Name NewName, DateTime RenamedOn) : DomainEvent(RenamedOn);
