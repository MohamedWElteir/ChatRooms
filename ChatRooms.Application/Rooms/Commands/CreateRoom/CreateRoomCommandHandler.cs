using ChatRooms.Application.Abstractions.Common;
using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork, IGenerator<RoomCode> codeGenerator, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateRoomCommand, Result<RoomDto>>
{
    public async Task<Result<RoomDto>> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var createResult = Room.Create(
            name: Name.From(command.Name),
            capacity: Capacity.From(command.Capacity),
            roomCode: codeGenerator.Generate(),
            dateTime: dateTimeProvider.UtcNow);

        if (createResult.IsFailure) return createResult.Error!;

        var room = createResult.Value!;
        await roomRepository.AddAsync(room, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RoomDto(
            Id: room.Id,
            Name: room.Name,
            Code: room.Code,
            Capacity: room.Capacity,
            CurrentParticipantsCount: room.CurrentParticipantsCount,
            Status: room.Status.ToString(),
            Version: room.Version
            );
    }
}
