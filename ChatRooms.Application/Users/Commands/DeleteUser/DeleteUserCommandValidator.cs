using ChatRooms.Domain.Shared.Enums;
using FluentValidation;

namespace ChatRooms.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage($"Reason must be a valid DeletionReason. Valid values: {string.Join(", ", Enum.GetNames<DeletionReason>())}");
    }
}
