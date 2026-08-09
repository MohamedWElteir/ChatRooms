using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.RoomParticipants.Commands;
using ChatRooms.Domain.RoomParticipants;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;

namespace ChatRooms.Application.Rooms.Commands.JoinRoom;

public sealed class JoinRoomCommandHandler(
    IRoomRepository roomRepository,
    IRoomParticipantRepository roomParticipantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<JoinRoomCommand, Result>
{
    public async Task<Result> Handle(JoinRoomCommand command, CancellationToken cancellationToken)
    {
        var alreadyJoined = await roomParticipantRepository.ExistsAsync(
            command.RoomId,
            command.UserId,
            cancellationToken);

        if (alreadyJoined)
            return RoomParticipantErrors.AlreadyJoined;

        var room = await roomRepository.GetByIdAsync(command.RoomId, cancellationToken);

        if (room is null)
            return RoomErrors.NotFound;

        var joinResult = room.Join(command.OccurredAt);

        if (joinResult.IsFailure)
            return joinResult;

        var participantResult = RoomParticipant.Create(
            command.RoomId,
            command.UserId,
            command.OccurredAt);

        if (participantResult.IsFailure)
            return participantResult.Error!;

        await roomParticipantRepository.AddAsync(participantResult.Value!, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
