using FluentValidation;

namespace ChatRooms.Application.Users.Commands.RenameUser;

public sealed class RenameUserCommandValidator : AbstractValidator<RenameUserCommand>
{
    public RenameUserCommandValidator()
    {
        RuleFor(x => x.NewName)
            .NotEmpty().WithMessage("New name cannot be empty.")
            .MaximumLength(15).WithMessage($"New name cannot exceed 15 characters.")
            .Must(name => !name.Any(char.IsWhiteSpace)).WithMessage("New name cannot contain whitespace.");
    }
}
