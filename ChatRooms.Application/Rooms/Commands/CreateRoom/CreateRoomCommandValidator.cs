using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.CreateRoom;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room name is required.")
            .MaximumLength(50).WithMessage("Room name cannot exceed 50 characters.")
            .Must(name => char.IsLetter(name[0])).WithMessage("Room name must start with a letter.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Capacity cannot exceed 100.");
    }
}