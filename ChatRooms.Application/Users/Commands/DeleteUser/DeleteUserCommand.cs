using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;

namespace ChatRooms.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid Id, DeletionReason Reason) : ICommand<Result>;
