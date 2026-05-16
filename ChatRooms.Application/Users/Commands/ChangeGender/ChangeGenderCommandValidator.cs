using ChatRooms.Domain.Users.Enums;
using FluentValidation;

namespace ChatRooms.Application.Users.Commands.ChangeGender;

public sealed class ChangeGenderCommandValidator : AbstractValidator<ChangeGenderCommand>
{
    public ChangeGenderCommandValidator()
    {
        RuleFor(x => x.NewGender)
            .IsInEnum().WithMessage($"Gender must be a valid Gender. Valid values: {string.Join(", ", Enum.GetNames<Gender>())}");
    }
}
