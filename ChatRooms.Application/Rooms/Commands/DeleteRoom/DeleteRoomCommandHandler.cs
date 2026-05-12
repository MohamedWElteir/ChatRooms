using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using MediatR;

namespace ChatRooms.Application.Rooms.Commands.DeleteRoom;

public sealed record DeleteRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoomCommand, Unit>
{
    public async Task<Unit> Handle(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetById(command.RoomId, cancellationToken) ?? throw new InvalidOperationException($"Room with id {command.RoomId} not found.");
        room.Delete(Enum.TryParse<DeletionReason>(command.DeletionReason, ignoreCase: true, out var deletionReason) ? deletionReason : throw new InvalidOperationException("Invalid deletion reason."), DateTimeUtc.NowUtc());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}