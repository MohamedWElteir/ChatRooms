using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Errors;
namespace ChatRooms.Application.Rooms.Commands.ChangeRoomCapacity;

public sealed class ChangeRoomCapacityCommandHandler(IRoomRepository roomRepository, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeRoomCapacityCommand, Result>
{
    public async Task<Result> Handle(ChangeRoomCapacityCommand request, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null) return RoomErrors.NotFound;

        var result = room.ChangeCapacity(Capacity.From(request.NewCapacity), DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
