using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.Enums;

namespace ChatRooms.Application.Users.Commands.ChangeGender;

public sealed record ChangeGenderCommand(Guid Id, Gender NewGender) : ICommand<Result>;
