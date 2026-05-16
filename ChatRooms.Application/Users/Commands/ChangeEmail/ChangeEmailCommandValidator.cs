using FluentValidation;

namespace ChatRooms.Application.Users.Commands.ChangeEmail;

public sealed class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
{
    public ChangeEmailCommandValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
