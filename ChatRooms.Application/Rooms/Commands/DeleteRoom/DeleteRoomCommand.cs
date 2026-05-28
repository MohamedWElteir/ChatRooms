using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.DeleteRoom;

public sealed record DeleteRoomCommand(RoomId RoomId, string DeletionReason) : ICommand<Result>;