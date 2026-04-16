using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using MediatR;
namespace ChatRooms.Application.Rooms.Commands.ChangeRoomCapacity;

public sealed class ChangeRoomCapacityCommandHandler(IRoomRepository roomRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeRoomCapacityCommand, Unit>
{
    public async Task<Unit> Handle(ChangeRoomCapacityCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetById(request.RoomId, cancellationToken) ?? throw new Exception("Room not found");
        room.ChangeCapacity(Capacity.From(request.NewCapacity), DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
