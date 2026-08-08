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

        var versionGuard = Builders<RoomDto>.Filter.And(
            Builders<RoomDto>.Filter.Eq(r => r.Id, newRoomDto.Id),
            Builders<RoomDto>.Filter.Lt(r => r.Version, newRoomDto.Version));

        var result = await readDbContext.Rooms.ReplaceOneAsync(
            versionGuard,
            newRoomDto,
            cancellationToken: cancellationToken);

        if (result.MatchedCount != 0) return;

        await readDbContext.Rooms.UpdateOneAsync(
            Builders<RoomDto>.Filter.Eq(r => r.Id, newRoomDto.Id),
            Builders<RoomDto>.Update
                .SetOnInsert(r => r.Name, newRoomDto.Name)
                .SetOnInsert(r => r.Code, newRoomDto.Code)
                .SetOnInsert(r => r.Capacity, newRoomDto.Capacity)
                .SetOnInsert(r => r.CurrentParticipantsCount, newRoomDto.CurrentParticipantsCount)
                .SetOnInsert(r => r.Status, newRoomDto.Status)
                .SetOnInsert(r => r.Version, newRoomDto.Version),
            new UpdateOptions { IsUpsert = true },
            cancellationToken: cancellationToken);
    }
}