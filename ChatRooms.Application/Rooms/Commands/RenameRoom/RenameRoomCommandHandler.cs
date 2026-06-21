using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;

namespace ChatRooms.Application.Rooms.Commands.RenameRoom;

public sealed class RenameRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RenameRoomCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RenameRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetById(command.Id, cancellationToken);
        if (room is null) return RoomErrors.NotFound;

        var renameResult = room.Rename(command.NewName, dateTimeProvider.UtcNow);
        if (renameResult.IsFailure)
        {
            (_, Error? renameError) = renameResult;
            return renameError!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return room.Name.Value;
    }
}
