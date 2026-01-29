using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomArchivedDomainEvent(RoomId RoomId, DateTime ArchivedAt) : DomainEvent(ArchivedAt);