using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.ChangeRoomCapacity;

public sealed record ChangeRoomCapacityCommand(RoomId RoomId, int NewCapacity) : ICommand<Result>;