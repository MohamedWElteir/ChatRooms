using ChatRooms.Blazor.HttpClients;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Blazor.Stores;

public sealed class RoomStore(IRoomApiClient api) : EntityStore<RoomListItem>
{
    public async Task LoadRoomsAsync(CancellationToken ct = default)
    {
        var rooms = await api.GetAllAsync(ct);
        ReplaceAll(rooms);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken ct = default)
    {
        var room = await api.CreateAsync(request, ct);
        InsertAt(0, new RoomListItem(
            room.Id, room.Name, room.Code, room.Capacity, room.CurrentParticipantsCount));
        return room;
    }

    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await api.GetByIdAsync(id, ct);

    public async Task RenameAsync(Guid id, string newName, CancellationToken ct = default)
    {
        await api.RenameAsync(id, newName, ct);
        UpdateWhere(r => r.Id == id, r => r with { Name = newName });
    }

    public async Task ChangeCapacityAsync(Guid id, int newCapacity, CancellationToken ct = default)
    {
        await api.ChangeCapacityAsync(id, newCapacity, ct);
        UpdateWhere(r => r.Id == id, r => r with { Capacity = newCapacity });
    }

    public async Task DeleteAsync(Guid id, string reason, CancellationToken ct = default)
    {
        await api.DeleteAsync(id, reason, ct);
        RemoveWhere(r => r.Id == id);
    }
}
