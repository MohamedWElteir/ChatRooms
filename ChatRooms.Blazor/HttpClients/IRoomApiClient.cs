using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Blazor.HttpClients;

public interface IRoomApiClient
{
    Task<IReadOnlyList<RoomListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task RenameAsync(Guid id, string newName, CancellationToken cancellationToken = default);
    Task ChangeCapacityAsync(Guid id, int newCapacity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string reason, CancellationToken cancellationToken = default);
}

public sealed record CreateRoomRequest(string Name, int Capacity);