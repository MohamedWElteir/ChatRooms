using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetRoomById;

public sealed record GetRoomByIdQuery(Guid Id) : IQuery<RoomDto>;