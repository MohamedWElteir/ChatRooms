using ChatRooms.Domain.Rooms.Events;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;

using MongoDB.Driver;

using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomRenamedProjector(
    ReadDbContext readDbContext,
    JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomRenamedDomainEvent>(
            eventContent, jsonOptions);
        if (domainEvent is null) return;

        var filter = Builders<RoomDto>.Filter.And(
            Builders<RoomDto>.Filter.Eq(r => r.Id, domainEvent.RoomId),
            Builders<RoomDto>.Filter.Lt(r => r.Version, domainEvent.AggregateVersion)
        );

        var result = await readDbContext.Rooms.UpdateOneAsync(
            filter,
            Builders<RoomDto>.Update
                .Set(r => r.Name, domainEvent.NewName)
                .Set(r => r.Version, domainEvent.AggregateVersion),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            var exists = await readDbContext.Rooms
                .Find(r => r.Id == (Guid)domainEvent.RoomId)
                .AnyAsync(cancellationToken);

            if (!exists)
                throw new InvalidOperationException(
                    $"Room {domainEvent.RoomId} not found in read model.");

        }
    }
}