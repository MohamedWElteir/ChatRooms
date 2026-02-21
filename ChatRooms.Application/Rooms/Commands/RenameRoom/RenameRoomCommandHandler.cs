using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.RenameRoom;

public sealed class RenameRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider) : ICommandHandler<RenameRoomCommand, string>
{
    public async Task<string> Handle(RenameRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetById(command.Id, cancellationToken) ?? throw new InvalidOperationException($"Room with id {command.Id} not found.");

        room.Rename(Name.From(command.NewName), DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return room.Name;
    }
}
