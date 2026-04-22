using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.ValueObjects;
using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.CreateRoom;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator(IRoomCapacityPolicy roomCapacityPolicy)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room name is required.")
            .MaximumLength(Name.MaxLength).WithMessage($"Room name cannot exceed {Name.MaxLength} characters.")
            .Must(name => char.IsLetter(name[0])).WithMessage("Room name must start with a letter.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(roomCapacityPolicy.MinCapacity).WithMessage($"Capacity must be at least {roomCapacityPolicy.MinCapacity}.")
            .LessThanOrEqualTo(roomCapacityPolicy.MaxCapacity).WithMessage($"Capacity cannot exceed {roomCapacityPolicy.MaxCapacity}.");

        RuleFor(x => x.CurrentParticipantsCount)
            .GreaterThan(0).WithMessage("Current Participants Count must be greater than 0.");
    }
}