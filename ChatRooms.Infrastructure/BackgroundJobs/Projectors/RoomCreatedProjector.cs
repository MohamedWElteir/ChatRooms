using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomCreatedProjector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomCreatedDomainEvent>(eventContent, jsonOptions);
        if (domainEvent is null) return;

        var newRoomDto = new RoomDto(
            domainEvent.RoomId,
            domainEvent.Name,
            domainEvent.Code,
            domainEvent.Capacity,
            1,
            nameof(RoomStatus.Active),
            1);

        await readDbContext.Rooms.InsertOneAsync(newRoomDto, cancellationToken: cancellationToken);
    }
}