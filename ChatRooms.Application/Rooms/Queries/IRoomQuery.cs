using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries;

public interface IRoomQuery : IQueryService<RoomDto?>
{
    Task<RoomDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomListItem>> GetAllAsync(CancellationToken cancellationToken);
}
