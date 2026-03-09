using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.Outbox;
using ChatRooms.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(
                IServiceScopeFactory scopeFactory,
                ILogger<OutboxProcessor> logger,
                IDateTimeProvider dateTimeProvider,
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
            await ProcessMessageAsync(message, scope.ServiceProvider, stoppingToken);

        await writeDbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(OutboxMessage message, IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        if (message.RetryCount >= _options.MaxRetryCount)
        {
            if (logger.IsEnabled(LogLevel.Critical))
                logger.LogCritical("Message {MessageId} of type {EventType} exceeded max retry count. Moving to Dead Letter Queue.",message.Id, message.Type);
            message.MarkAsDeadLetter(DateTimeUtc.FromUtc(dateTimeProvider.UtcNow), "Max retries exceeded.");
            return;
        }

        var projector = serviceProvider.GetKeyedService<IEventProjector>(message.Type);

        if (projector is null)
        {
            logger.LogWarning("No projector found for event type {EventType}",message.Type);
            return;
        }

        try
        {
            if (logger.IsEnabled(LogLevel.Information))
                 logger.LogInformation("Processing outbox message {MessageId} of type {EventType}.", message.Id, message.Type);

            await projector.ProjectAsync(message.Content, stoppingToken);
            message.MarkAsProcessed(DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to project event {EventType} for message {MessageId}.", message.Type, message.Id);
            message.RecordFailure(ex.Message);
        }
    }
}