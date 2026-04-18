using ChatRooms.DTOs.Shared;

namespace ChatRooms.DTOs.Rooms;

public sealed record RoomListItem(Guid Id, string Name, string Code, int Capacity, int CurrentParticipantsCount) : ListItemBase(Id);
