using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using System.Text.Json;
using MongoDB.Driver;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomCreatedProjector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomCreatedDomainEvent>(eventContent, jsonOptions);
        if (domainEvent is null)
            throw new JsonException(
            "Unable to deserialize RoomCreatedDomainEvent.");

        var newRoomDto = new RoomDto(
            Id: domainEvent.RoomId,
            Name: domainEvent.Name,
            Code: domainEvent.Code,
            Capacity: domainEvent.Capacity,
            CurrentParticipantsCount: domainEvent.CurrentParticipantsCount,
            Status: nameof(RoomStatus.Active),
            Version: domainEvent.AggregateVersion);

        await readDbContext.Rooms.ReplaceOneAsync(
            r => r.Id == newRoomDto.Id,
            newRoomDto,
            new ReplaceOptions
            {
                IsUpsert = true
            },
            cancellationToken: cancellationToken);
    }
}