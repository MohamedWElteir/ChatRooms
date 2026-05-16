using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOutboxMessageProcessor messageProcessor,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> options) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

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
            await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var writeDbContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

        var messages = await writeDbContext.Set<OutboxMessage>()
            .Where(m => !m.IsProcessed && !m.IsDeadLetter)
            .OrderBy(m => m.OccurredOn)
            .Take(_options.BatchSize)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            var projector = scope.ServiceProvider.GetKeyedService<IEventProjector>(message.Type);
            await messageProcessor.ProcessAsync(message, projector, stoppingToken);
        }

        await writeDbContext.SaveChangesAsync(stoppingToken);
    }
}