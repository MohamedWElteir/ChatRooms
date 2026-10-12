using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.RestoreRoom;

public sealed record RestoreRoomCommand(RoomId RoomId) : ICommand<Result>;
