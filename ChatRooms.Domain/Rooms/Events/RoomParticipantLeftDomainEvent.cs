using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomParticipantLeftDomainEvent(
    DateTimeUtc LeftAt
) : DomainEvent(LeftAt);