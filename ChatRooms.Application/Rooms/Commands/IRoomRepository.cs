using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Application.Rooms.Commands;

public interface IRoomRepository
{
    Task<Room?> GetById(RoomId id, CancellationToken cancellationToken);
    Task<Room?> GetByCode(RoomCode code, CancellationToken cancellationToken);
    Task Add(Room room, CancellationToken cancellationToken);
    Task<bool> Exists(RoomCode code, CancellationToken cancellationToken);
}
