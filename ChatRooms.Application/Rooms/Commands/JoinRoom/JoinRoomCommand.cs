using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.ValueObjects;


namespace ChatRooms.Application.Rooms.Commands.JoinRoom;

public sealed record JoinRoomCommand(
    RoomId RoomId,
    UserId UserId,
    DateTimeUtc OccurredAt) : ICommand<Result>;