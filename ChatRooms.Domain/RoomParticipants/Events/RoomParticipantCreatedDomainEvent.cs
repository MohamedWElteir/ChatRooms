using ChatRooms.Domain.RoomParticipants.ValueObjects;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.RoomParticipants.Events;

public sealed record RoomParticipantCreatedDomainEvent(
    RoomParticipantId RoomMemberId,
    RoomId RoomId,
    UserId UserId,
    DateTimeUtc JoinedAt) : DomainEvent(JoinedAt);
