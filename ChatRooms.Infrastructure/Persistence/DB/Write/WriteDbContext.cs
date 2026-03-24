using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChatRooms.Infrastructure.Persistence.DB.Write;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options, JsonSerializerOptions jsonOptions) : DbContext(options), IUnitOfWork
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
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
                var outboxMessage = OutboxMessage.Create(
                    type: domainEvent.GetType().Name!,
                    content: JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions),
                    occurredOn: domainEvent.OccurredAt
                );

                OutboxMessages.Add(outboxMessage);
            }
            aggregate.ClearDomainEvents();
        }
    }
}