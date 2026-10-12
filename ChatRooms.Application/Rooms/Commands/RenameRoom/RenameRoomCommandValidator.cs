using ChatRooms.Domain.Rooms.ValueObjects;

using FluentValidation;

namespace ChatRooms.Application.Rooms.Commands.RenameRoom;

public sealed class RenameRoomCommandValidator : AbstractValidator<RenameRoomCommand>
{
    public RenameRoomCommandValidator()
    {
        RuleFor(x => x.NewName)
            .NotEmpty().WithMessage("New name cannot be empty.")
            .MaximumLength(Name.MaxLength).WithMessage($"New name cannot exceed {Name.MaxLength} characters.")
            .Must(name => !string.IsNullOrWhiteSpace(name) && char.IsLetter(name[0])).WithMessage("New name must start with a letter.");
    }
}
