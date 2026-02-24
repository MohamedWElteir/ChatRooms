using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Application.Rooms.DTOs;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork, IRoomCodeGenerator codeGenerator, IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var room = Room.Create(
            name: Name.From(command.Name),
            capacity: Capacity.From(command.Capacity),
            roomCode: codeGenerator.Generate(),
            dateTime: DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));

        await roomRepository.Add(room, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RoomDto(
            Id: room.Id,
            Name: room.Name,
            Code: room.Code,
            Capacity: room.Capacity,
            CurrentParticipantsCount: room.CurrentParticipantsCount,
            Status: room.Status.ToString());
    }
}
