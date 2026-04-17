using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetRoomByCode;

public sealed record GetRoomByCodeQuery(string Code) : IQuery<RoomDto>;
