using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.RenameRoom;

public sealed record RenameRoomCommand(RoomId Id, string NewName) : ICommand<Result<string>>;
