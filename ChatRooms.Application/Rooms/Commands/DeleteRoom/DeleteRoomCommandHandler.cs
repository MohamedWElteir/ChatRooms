using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Shared.Errors;

namespace ChatRooms.Application.Rooms.Commands.DeleteRoom;

public sealed class DeleteRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<DeleteRoomCommand, Result>
{
    public async Task<Result> Handle(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(command.RoomId, cancellationToken);
        if (room is null) return RoomErrors.NotFound;

        if (!Enum.TryParse<DeletionReason>(command.DeletionReason, ignoreCase: true, out var deletionReason))
            return RoomErrors.InvalidDeletionReason;

        var result = room.Delete(deletionReason, dateTimeProvider.UtcNow);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}