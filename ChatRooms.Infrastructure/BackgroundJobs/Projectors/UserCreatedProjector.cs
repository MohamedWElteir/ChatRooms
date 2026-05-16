using ChatRooms.Domain.Users.Events;
using ChatRooms.DTOs.Users;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class UserCreatedProjector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<UserCreatedDomainEvent>(eventContent, jsonOptions);
        if (domainEvent is null) return;

        var newUserDto = new UserDto(
            Id: domainEvent.UserId,
            Name: domainEvent.Name,
            Email: domainEvent.Email,
            Gender: domainEvent.Gender.ToString(),
            Version: domainEvent.AggregateVersion);

        await readDbContext.Users.InsertOneAsync(newUserDto, cancellationToken: cancellationToken);
    }
}
