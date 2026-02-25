namespace ChatRooms.Application.Rooms.DTOs;

public sealed record RoomListItemDto(Guid Id, string Name, string Code, int Capacity, int CurrentParticipantsCount);
