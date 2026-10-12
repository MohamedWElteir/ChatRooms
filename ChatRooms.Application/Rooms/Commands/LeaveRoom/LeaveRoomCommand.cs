using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.LeaveRoom;

public sealed record LeaveRoomCommand(RoomId RoomId) : ICommand<Result>;
