using ChatRooms.Domain.Users.Events;
using ChatRooms.DTOs.Users;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using MongoDB.Driver;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class UserDeletedProjector(
    ReadDbContext readDbContext,
    JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<UserDeletedDomainEvent>(
            eventContent, jsonOptions);
        if (domainEvent is null) return;

        var filter = Builders<UserDto>.Filter.And(
            Builders<UserDto>.Filter.Eq(u => u.Id, domainEvent.UserId.Value),
            Builders<UserDto>.Filter.Lt(u => u.Version, domainEvent.AggregateVersion)
        );

        await readDbContext.Users.DeleteOneAsync(
            filter,
            cancellationToken: cancellationToken);
    }
}
