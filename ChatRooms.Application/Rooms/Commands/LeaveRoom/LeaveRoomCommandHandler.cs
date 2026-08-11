using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.RoomParticipants.Commands;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.LeaveRoom;

public sealed class LeaveRoomCommandHandler(
    IRoomRepository roomRepository,
    IRoomParticipantRepository roomParticipantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<LeaveRoomCommand, Result>
{
    public async Task<Result> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(request.RoomId, cancellationToken) 
                ?? throw new InvalidOperationException("Room not found");
        room.Leave(request.OccurredAt);
        var roomParticipant = await roomParticipantRepository.GetByIdAsync(request.RoomId, request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Room participant not found");
        roomParticipant.Leave(request.OccurredAt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
