using FluentValidation;

namespace ChatRooms.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("User name is required.")
            .MaximumLength(15).WithMessage($"User name cannot exceed 15 characters.")
            .Must(name => !name.Any(char.IsWhiteSpace)).WithMessage("User name cannot contain whitespace.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required.")
            .LessThan(DateTime.UtcNow).WithMessage("Birth date cannot be in the future.");
    }
}
