using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Application.Rooms.Commands.RenameRoom;

public sealed record RenameRoomCommand(RoomId Id, string NewName) : ICommand<string>;
