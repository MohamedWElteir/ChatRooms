using ChatRooms.Application.Rooms.DTOs;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Infrastructure.Persistence.Read;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomCreatedProjector(ReadDbContext readDbContext) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomCreatedDomainEvent>(eventContent);
        if (domainEvent is null) return;

        var newRoomDto = new RoomDto(
            domainEvent.RoomId.Value,
            domainEvent.Name,
            domainEvent.Code,
            domainEvent.Capacity,
            1,
            nameof(RoomStatus.Active));

        await readDbContext.Rooms.InsertOneAsync(newRoomDto, cancellationToken: cancellationToken);
    }
}