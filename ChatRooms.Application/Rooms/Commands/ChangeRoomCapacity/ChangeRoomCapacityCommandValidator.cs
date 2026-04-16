using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.ChangeRoomCapacity;

public sealed class ChangeRoomCapacityCommandValidator : AbstractValidator<ChangeRoomCapacityCommand>
{
    public ChangeRoomCapacityCommandValidator()
    {
        RuleFor(x => x.NewCapacity)
            .GreaterThan(0).WithMessage("New capacity must be greater than 0.");
    }
}
