using ChatRooms.DTOs.Shared;

namespace ChatRooms.DTOs.RoomParticipants;

public sealed record RoomParticipantDto(
    Guid Id, 
    Guid RoomId, 
    Guid UserId, 
    DateTime JoinedAt, 
    DateTime? LeftAt, 
    int Version) : DtoBase(Id, Version);
