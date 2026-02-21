using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Rooms.DTOs;

namespace ChatRooms.Application.Rooms.Queries.GetRoomByCode;

public sealed record GetRoomByCodeQuery(string Code) : IQuery<RoomDto>;
