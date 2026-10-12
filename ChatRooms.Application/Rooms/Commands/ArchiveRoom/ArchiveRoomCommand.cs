using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.ArchiveRoom;

public sealed record ArchiveRoomCommand(RoomId RoomId) : ICommand<Result>;
