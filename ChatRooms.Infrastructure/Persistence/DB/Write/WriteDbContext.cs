using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.DB.Write;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options, IOutboxMessageFactory outboxMessageFactory) : DbContext(options), IUnitOfWork
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
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(x => x.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count != 0)
            .ToList();

        var messages = outboxMessageFactory.CreateOutboxMessages(aggregates);
        OutboxMessages.AddRange(messages);

        return await base.SaveChangesAsync(cancellationToken);
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return SaveChangesAsync(cancellationToken);
    }
}