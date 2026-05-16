using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Queries.GetUserByEmail;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserDto>;
