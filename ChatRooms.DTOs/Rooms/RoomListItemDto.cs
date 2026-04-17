namespace ChatRooms.DTOs.Rooms;

public sealed record RoomListItemDto(Guid Id, string Name, string Code, int Capacity, int CurrentParticipantsCount);
