using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;
