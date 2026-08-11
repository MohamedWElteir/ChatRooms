using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.Outbox;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs the lease-heartbeated batch loop for the outbox. Before every message a
/// heartbeat renews the lease of the remaining messages; if renewal loses even one
/// message, ownership of the batch is gone and processing stops immediately
/// (duplicate projections are prevented by abandoning rather than racing).
/// </summary>
public sealed class OutboxBatchProcessor(
    IOutboxMessageProcessor messageProcessor,
    IOptions<OutboxOptions> options,
    ILogger<OutboxBatchProcessor> logger)
{
    private readonly OutboxOptions _options = options.Value;

    public async Task ProcessBatchAsync(
        IReadOnlyList<OutboxMessage> messages,
        IOutboxRepository outboxRepository,
        string workerId,
        Func<string, IEventProjector?> projectorResolver,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (messages.Count == 0) return;

        var leaseDuration = TimeSpan.FromMinutes(
            _options.ProcessingLeaseDurationMinutes);

        var allMessageIds = messages.Select(m => m.Id).ToArray();

        for (var index = 0; index < messages.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remainingIds = allMessageIds.AsSpan(index).ToArray();

            var renewed = await outboxRepository.RenewLeaseAsync(
                remainingIds,
                workerId,
                leaseDuration,
                cancellationToken);

            if (renewed != remainingIds.Length)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Outbox worker {WorkerId} lost its lease on {Lost} of {Total} " +
                        "remaining message(s); abandoning the rest of the batch.",
                        workerId,
                        remainingIds.Length - renewed,
                        remainingIds.Length);
                }

                return;
            }

            var message = messages[index];

            await messageProcessor.ProcessAsync(
                message,
                projectorResolver(message.Type),
                cancellationToken);
        }
    }
}