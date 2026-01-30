using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomRestoredDomainEvent(
    DateTimeUtc RestoredAt
) : DomainEvent(RestoredAt);