using ChatRooms.DTOs.Shared;

namespace ChatRooms.DTOs.Rooms;

public sealed record RoomDto(Guid Id, string Name, string Code, int Capacity, int CurrentParticipantsCount, string Status, int Version) : DtoBase(Id, Version);