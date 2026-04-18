using ChatRooms.Domain.Rooms.Events;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;

using MongoDB.Driver;

using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class RoomRenamedProjector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<RoomRenamedDomainEvent>(eventContent, jsonOptions);
        if (domainEvent is null) return;

        var result = await readDbContext.Rooms.UpdateOneAsync(
            filter: room => room.Id == domainEvent.RoomId,
            update: Builders<RoomDto>.Update
                .Set(room => room.Name, domainEvent.NewName)
                .Inc(room => room.Version, 1),
            cancellationToken: cancellationToken);
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Room with id {domainEvent.RoomId} not found in read database.");
        }
    }
}
