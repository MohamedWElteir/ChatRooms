using ChatRooms.Application.Rooms.DTOs;

namespace ChatRooms.Application.Rooms.Queries;

public interface IRoomQuery
{
    Task<RoomDto?> GetById(Guid id, CancellationToken cancellationToken);
    Task<RoomDto?> GetByCode(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomListItemDto>> GetAll(CancellationToken cancellationToken);
}
