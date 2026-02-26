using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChatRooms.Infrastructure.Persistence.Write;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages();

        return await base.SaveChangesAsync(cancellationToken);
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages()
    {
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(x => x.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count != 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var outboxMessage = new OutboxMessage(
                    Id: Guid.NewGuid(),
                    Type: domainEvent.GetType().FullName!,
                    Content: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    ErrorMessage: null,
                    OccurredOn: domainEvent.OccurredAt,
                    ProcessedOn: null,
                    RetryCount: 0,
                    IsProcessed: false
                );

                OutboxMessages.Add(outboxMessage);
            }
            aggregate.ClearDomainEvents();
        }
    }
}