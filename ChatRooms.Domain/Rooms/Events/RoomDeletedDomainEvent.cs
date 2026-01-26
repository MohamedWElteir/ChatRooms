using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomDeletedDomainEvent(RoomId RoomId, DeleteCause DeleteReason, DateTime DeletedAt) : DomainEvent(DeletedAt);