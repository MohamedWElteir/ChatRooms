using ChatRooms.Domain.Users.Events;
using ChatRooms.DTOs.Users;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using MongoDB.Driver;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class UserGenderChangedProjector(
    ReadDbContext readDbContext,
    JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<UserGenderChangedDomainEvent>(
            eventContent, jsonOptions);
        if (domainEvent is null) return;

        var filter = Builders<UserDto>.Filter.And(
            Builders<UserDto>.Filter.Eq(u => u.Id, domainEvent.UserId.Value),
            Builders<UserDto>.Filter.Lt(u => u.Version, domainEvent.AggregateVersion)
        );

        var result = await readDbContext.Users.UpdateOneAsync(
            filter,
            Builders<UserDto>.Update
                .Set(u => u.Gender, domainEvent.NewGender.ToString())
                .Set(u => u.Version, domainEvent.AggregateVersion),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            var exists = await readDbContext.Users
                .Find(u => u.Id == domainEvent.UserId.Value)
                .AnyAsync(cancellationToken);

            if (!exists)
                throw new InvalidOperationException(
                    $"User {domainEvent.UserId} not found in read model.");
        }
    }
}
