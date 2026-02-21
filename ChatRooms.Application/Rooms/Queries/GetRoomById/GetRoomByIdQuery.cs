using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Rooms.DTOs;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Application.Rooms.Queries.GetRoomById;

public sealed record GetRoomByIdQuery(Guid Id) : IQuery<RoomDto>;