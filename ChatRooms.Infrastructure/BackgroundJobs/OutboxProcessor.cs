using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.Persistence.Outbox;
using ChatRooms.Infrastructure.Persistence.Read;
using ChatRooms.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing outbox messages.");
            }
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var writeDbContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDbContext = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

        var messageIds = await writeDbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.ProcessedOn)
            .Take(20)
            .Select(m => m.Id)
            .ToListAsync(stoppingToken);

        if (messageIds.Count == 0) return;

        await writeDbContext.Set<OutboxMessage>()
            .Where(m => messageIds.Contains(m.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.ProcessedOn, m => DateTimeUtc.FromUtc(DateTime.UtcNow)),
                stoppingToken);
    }
}