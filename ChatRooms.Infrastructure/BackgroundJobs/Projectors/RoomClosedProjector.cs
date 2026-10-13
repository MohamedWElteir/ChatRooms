using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using MongoDB.Driver;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomClosedProjector(
    ReadDbContext readDbContext,
    JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomClosedDomainEvent>(
            eventContent, jsonOptions);
        if (domainEvent is null) return;

        var filter = Builders<RoomDto>.Filter.And(
            Builders<RoomDto>.Filter.Eq(r => r.Id, domainEvent.RoomId.Value),
            Builders<RoomDto>.Filter.Lt(r => r.Version, domainEvent.AggregateVersion)
        );

        await readDbContext.Rooms.UpdateOneAsync(
            filter,
            Builders<RoomDto>.Update
                .Set(r => r.Status, nameof(RoomStatus.Archived))
                .Set(r => r.Version, domainEvent.AggregateVersion),
            cancellationToken: cancellationToken);
    }
}
