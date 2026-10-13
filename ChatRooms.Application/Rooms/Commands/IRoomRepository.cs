using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Application.Rooms.Commands;

public interface IRoomRepository : IRepository<Room, RoomId>
{
    Task<Room?> GetByCode(RoomCode code, CancellationToken cancellationToken);
    Task<bool> Exists(RoomCode code, CancellationToken cancellationToken);
}
