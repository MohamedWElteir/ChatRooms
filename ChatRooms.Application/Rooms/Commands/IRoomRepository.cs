using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Application.Rooms.Commands;

public interface IRoomRepository : IRepository<Room, RoomId>
{
    Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken);
}
