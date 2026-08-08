using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessor(
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxMessageProcessor> logger) : IOutboxMessageProcessor
{
    private readonly OutboxOptions _options = options.Value;

    public async Task ProcessAsync(OutboxMessage message, IEventProjector? projector, CancellationToken cancellationToken)
    {
        if (message.RetryCount >= _options.MaxRetryCount)
        {
            if (logger.IsEnabled(LogLevel.Critical))
                logger.LogCritical("Message {MessageId} of type {EventType} exceeded max retry count. Moving to Dead Letter Queue.", message.Id, message.Type);
            message.MarkAsDeadLetter(DateTimeUtc.FromUtc(dateTimeProvider.UtcNow), "Max retries exceeded.");
            return;
        }

        if (projector is null)
        {
            logger.LogCritical(
                "No projector found for event type {EventType}. " +
                "Moving message {MessageId} to Dead Letter Queue.",
                message.Type,
                message.Id);

            message.MarkAsDeadLetter(
                DateTimeUtc.FromUtc(dateTimeProvider.UtcNow),
                $"No projector registered for event type '{message.Type}'.");
            return;
        }

        try
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Processing outbox message {MessageId} of type {EventType}.", message.Id, message.Type);

            await projector.ProjectAsync(message.Content, cancellationToken);
            message.MarkAsProcessed(DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to project event {EventType} for message {MessageId}.", message.Type, message.Id);
            message.RecordFailure(ex.Message);
        }
    }
}
