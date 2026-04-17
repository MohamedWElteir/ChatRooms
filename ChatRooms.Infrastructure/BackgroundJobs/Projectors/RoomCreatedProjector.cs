using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomCreatedProjector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomCreatedDomainEvent>(eventContent, _jsonOptions);
        if (domainEvent is null) return;

        var newRoomDto = new RoomDto(
            domainEvent.RoomId,
            domainEvent.Name,
            domainEvent.Code,
            domainEvent.Capacity,
            1,
            nameof(RoomStatus.Active));

        await readDbContext.Rooms.InsertOneAsync(newRoomDto, cancellationToken: cancellationToken);
    }
}