---
name: domain-event-projector-scaffold
description: Scaffold a domain event + its MongoDB projector + DI registration following ChatRooms outbox pattern. Use when adding a new domain event that needs to sync to the read model.
---

# Domain Event + Projector Scaffold

Each domain event needs three parts: the event record, the projector class, and the DI registration.

## Part 1: Domain Event

Create in `ChatRooms.Domain.{Entity}.Events`:

```csharp
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.{Entity}.Events;

public sealed record {Name}DomainEvent(
    // Event-specific value object parameters
    DateTimeUtc OccurredAt
    ) : DomainEvent(OccurredAt);
```

### Example: RoomCreatedDomainEvent
```csharp
public sealed record RoomCreatedDomainEvent(
    RoomId RoomId,
    Name Name,
    RoomCode Code,
    Capacity Capacity,
    int CurrentParticipantsCount,
    DateTimeUtc OccurredAt
    ) : DomainEvent(OccurredAt);
```

## Part 2: Aggregate wiring

Add to the aggregate's `Apply(IDomainEvent)` switch:

```csharp
case {Name}DomainEvent e:
    Apply(e);
    break;
```

Add the private applier method:

```csharp
private void Apply({Name}DomainEvent @event)
{
    // Set properties from @event
    UpdatedAt = @event.OccurredAt;
}
```

## Part 3: Event Projector

Create in `ChatRooms.Infrastructure.BackgroundJobs.Projectors`:

### Insert projector (new entity)
```csharp
using ChatRooms.Domain.{Entity}.Enums;
using ChatRooms.Domain.{Entity}.Events;
using ChatRooms.DTOs.{Entity}s;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using System.Text.Json;

namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public sealed class {Name}Projector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<{Name}DomainEvent>(eventContent, jsonOptions);
        if (domainEvent is null) return;

        var dto = new {Entity}Dto(
            // Map from domainEvent
            Version: domainEvent.AggregateVersion);

        await readDbContext.{Entity}s.InsertOneAsync(dto, cancellationToken: cancellationToken);
    }
}
```

### Update projector (existing entity)
```csharp
public sealed class {Name}Projector(ReadDbContext readDbContext, JsonSerializerOptions jsonOptions) : IEventProjector
{
    public async Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
    {
        var domainEvent = JsonSerializer.Deserialize<{Name}DomainEvent>(eventContent, jsonOptions);
        if (domainEvent is null) return;

        var filter = Builders<{Entity}Dto>.Filter.And(
            Builders<{Entity}Dto>.Filter.Eq(r => r.Id, domainEvent.{Entity}Id),
            Builders<{Entity}Dto>.Filter.Lt(r => r.Version, domainEvent.AggregateVersion)
        );

        var result = await readDbContext.{Entity}s.UpdateOneAsync(
            filter,
            Builders<{Entity}Dto>.Update
                .Set(r => r.{Property}, domainEvent.{Property})
                .Set(r => r.Version, domainEvent.AggregateVersion),
            cancellationToken: cancellationToken);
    }
}
```

## Part 4: DI Registration

Add to `ChatRooms.Infrastructure.DependencyInjection.AddInfrastructure()`:

```csharp
services.AddKeyedScoped<IEventProjector, {Name}Projector>(nameof({Name}DomainEvent));
```
