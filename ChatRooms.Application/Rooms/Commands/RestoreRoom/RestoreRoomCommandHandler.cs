using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;

namespace ChatRooms.Application.Rooms.Commands.RestoreRoom;

public sealed class RestoreRoomCommandHandler(
    IRoomRepository roomRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RestoreRoomCommand, Result>
{
    public async Task<Result> Handle(RestoreRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetAsync(command.RoomId, cancellationToken);

        var result = room.Restore(dateTimeProvider.UtcNow);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
