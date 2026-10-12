using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.JoinRoom;

public sealed record JoinRoomCommand(RoomId RoomId) : ICommand<Result>;
