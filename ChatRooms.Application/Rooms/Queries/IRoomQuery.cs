using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries;

public interface IRoomQuery
{
    Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<RoomDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomListItemDto>> GetAllAsync(CancellationToken cancellationToken);
}
