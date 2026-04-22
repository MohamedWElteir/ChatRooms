using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Commands.CreateRoom;

public sealed record CreateRoomCommand(string Name, int Capacity, int CurrentParticipantsCount) : ICommand<RoomDto>;