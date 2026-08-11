using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomParticipantCountDecrementedDomainEvent(RoomId RoomId, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);