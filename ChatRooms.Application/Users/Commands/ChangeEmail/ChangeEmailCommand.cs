using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Users.Commands.ChangeEmail;

public sealed record ChangeEmailCommand(Guid Id, string NewEmail) : ICommand<Result>;
