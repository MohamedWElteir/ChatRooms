namespace ChatRooms.DTOs.Rooms;

public sealed record RoomDto(Guid Id, string Name, string Code, int Capacity, int CurrentParticipantsCount, string Status);