using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomRenamedDomainEvent(RoomId RoomId, Name NewName, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);
