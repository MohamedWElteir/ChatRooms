using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(string Name, string Email, Gender Gender, DateTime BirthDate) : ICommand<Result<UserDto>>;
