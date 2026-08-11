using ChatRooms.Domain.RoomParticipants.ValueObjects;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.RoomParticipants.Events;

public sealed record RoomParticipantLeftDomainEvent(
    RoomParticipantId RoomParticipantId,
    RoomId RoomId,
    UserId UserId,
    DateTimeUtc LeftAt) : DomainEvent(LeftAt);