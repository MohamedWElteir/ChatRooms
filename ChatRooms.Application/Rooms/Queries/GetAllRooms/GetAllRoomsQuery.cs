using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetAllRooms;

public sealed record GetAllRoomsQuery() : IQuery<IReadOnlyList<RoomListItem>>;
