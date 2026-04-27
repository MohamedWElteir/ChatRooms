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
            Id: domainEvent.RoomId,
            Name: domainEvent.Name,
            Code: domainEvent.Code,
            Capacity: domainEvent.Capacity,
            CurrentParticipantsCount: 1,
            Status: nameof(RoomStatus.Active),
            Version: domainEvent.AggregateVersion);

        await readDbContext.Rooms.InsertOneAsync(newRoomDto, cancellationToken: cancellationToken);
    }
}