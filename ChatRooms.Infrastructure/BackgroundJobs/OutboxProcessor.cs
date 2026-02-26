using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Persistence.Outbox;
using ChatRooms.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger, IDateTimeProvider dateTimeProvider) : BackgroundService
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

        var messages = await writeDbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            var projector = scope.ServiceProvider.GetKeyedService<IEventProjector>(message.Type);

            if (projector is not null)
            {
                try
                {
                    if(logger.IsEnabled(LogLevel.Information) || logger.IsEnabled(LogLevel.Debug))
                        logger.LogInformation("Processing outbox message with ID: {MessageId} and Type: {EventType}", message.Id, message.Type);

                    await projector.ProjectAsync(message.Content, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while projecting event {EventType} with ID: {MessageId}", message.Type, message.Id);
                }
            }
            else
            {
                logger.LogWarning("No projector found for event type: {EventType}", message.Type);
            }
        }
        var messageIds = messages.Select(m => m.Id).ToList();

        await writeDbContext.Set<OutboxMessage>()
            .Where(m => messageIds.Contains(m.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.IsProcessed, m => true)
                .SetProperty(m => m.ProcessedOn, m => DateTimeUtc.FromUtc(dateTimeProvider.UtcNow)),
                stoppingToken);

    }
}