using ChatRooms.Domain.Shared.Enums;
using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.DeleteRoom;

public sealed class DeleteRoomCommandValidator : AbstractValidator<DeleteRoomCommand>
{
    public DeleteRoomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty();

        RuleFor(x => x.DeletionReason)
            .NotEmpty()
            .IsEnumName(typeof(DeletionReason), caseSensitive: false)
            .WithMessage($"Reason must be a valid DeletionReason (case-insensitive). Valid values: {string.Join(", ", Enum.GetNames<DeletionReason>())}");
    }
}