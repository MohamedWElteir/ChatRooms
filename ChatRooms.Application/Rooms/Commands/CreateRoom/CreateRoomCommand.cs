using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Rooms.DTOs;

namespace ChatRooms.Application.Rooms.Commands.CreateRoom;

public sealed record CreateRoomCommand(string Name, int Capacity) : ICommand<RoomDto>;