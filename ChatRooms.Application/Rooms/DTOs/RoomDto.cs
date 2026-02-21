namespace ChatRooms.Application.Rooms.DTOs;

public sealed record RoomDto(Guid Id, string Name, string Code, int Capacity, int CurrentParticipantsCount, string Status);