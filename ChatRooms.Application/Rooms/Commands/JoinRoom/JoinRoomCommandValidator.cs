using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.JoinRoom;

public sealed class JoinRoomCommandValidator : AbstractValidator<JoinRoomCommand>
{
    public JoinRoomCommandValidator()
    {
        RuleFor(x => x.RoomId.Value)
            .NotEmpty().WithMessage("Room id cannot be empty.");
    }
}
