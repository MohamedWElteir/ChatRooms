using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.LeaveRoom;

public sealed class LeaveRoomCommandValidator : AbstractValidator<LeaveRoomCommand>
{
    public LeaveRoomCommandValidator()
    {
        RuleFor(x => x.RoomId.Value)
            .NotEmpty().WithMessage("Room id cannot be empty.");
    }
}
