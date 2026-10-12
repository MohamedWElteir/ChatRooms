using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.ArchiveRoom;

public sealed class ArchiveRoomCommandValidator : AbstractValidator<ArchiveRoomCommand>
{
    public ArchiveRoomCommandValidator()
    {
        RuleFor(x => x.RoomId.Value)
            .NotEmpty().WithMessage("Room id cannot be empty.");
    }
}
