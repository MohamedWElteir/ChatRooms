using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.RestoreRoom;

public sealed class RestoreRoomCommandValidator : AbstractValidator<RestoreRoomCommand>
{
    public RestoreRoomCommandValidator()
    {
        RuleFor(x => x.RoomId.Value)
            .NotEmpty().WithMessage("Room id cannot be empty.");
    }
}
