using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomDeletedDomainEvent(RoomId RoomId, DeletionReason DeletionReason, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);