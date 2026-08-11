using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.LeaveRoom;

public sealed record LeaveRoomCommand(
    Guid RoomId,
    Guid UserId,
    DateTime OccurredAt) : ICommand<Result>;