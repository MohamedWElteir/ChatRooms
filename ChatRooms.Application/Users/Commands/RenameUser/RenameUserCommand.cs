using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Users.Commands.RenameUser;

public sealed record RenameUserCommand(Guid Id, string NewName) : ICommand<Result<string>>;
