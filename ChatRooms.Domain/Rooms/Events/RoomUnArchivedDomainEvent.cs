using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomUnArchivedDomainEvent(RoomId RoomId, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);