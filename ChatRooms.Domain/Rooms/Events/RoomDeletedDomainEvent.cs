using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomDeletedDomainEvent(RoomId RoomId, DeletionReason DeletionReason, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);