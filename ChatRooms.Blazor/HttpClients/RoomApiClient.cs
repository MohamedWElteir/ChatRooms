using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Blazor.HttpClients;

public sealed class RoomApiClient(HttpClient http) : IRoomApiClient
{
    public async Task<IReadOnlyList<RoomListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<RoomListItem>>("api/rooms", cancellationToken) ?? [];

    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<RoomDto>($"api/rooms/{id}", cancellationToken);

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/rooms", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var room = await response.Content.ReadFromJsonAsync<RoomDto>(cancellationToken);
        return room is null
            ? throw new InvalidOperationException("The server returned a successful response but no room payload was provided.")
            : room;
    }

    public async Task RenameAsync(Guid id, string newName, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/rooms/{id}/name", new { NewName = newName }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangeCapacityAsync(Guid id, int newCapacity, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/rooms/{id}/capacity", new { NewCapacity = newCapacity }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/rooms/{id}")
        {
            Content = JsonContent.Create(new { Reason = reason })
        };
        var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}