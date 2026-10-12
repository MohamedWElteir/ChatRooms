using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;

namespace ChatRooms.Application.Rooms.Commands.ArchiveRoom;

public sealed class ArchiveRoomCommandHandler(
    IRoomRepository roomRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ArchiveRoomCommand, Result>
{
    public async Task<Result> Handle(ArchiveRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetById(command.RoomId, cancellationToken);
        if (room is null)
            return RoomErrors.NotFound;

        var result = room.Archive(dateTimeProvider.UtcNow);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
