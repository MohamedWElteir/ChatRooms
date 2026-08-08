using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOutboxMessageProcessor messageProcessor,
    IOutboxRepository outboxRepository,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> options) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    private readonly string _workerId =  $"{Environment.MachineName}:{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation(
                    "Outbox processor is stopping due to cancellation request.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An error occurred while processing outbox messages.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(
                    _options.PollingIntervalSeconds),
                stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(
        CancellationToken cancellationToken)
    {
        var messages = await outboxRepository.ClaimBatchAsync(
            _options.BatchSize,
            _workerId,
            TimeSpan.FromMinutes(_options.ProcessingLeaseDurationMinutes),
            cancellationToken);

        if (messages.Count == 0)
            return;

        await using var scope =
            scopeFactory.CreateAsyncScope();

        foreach (var message in messages)
        {
            var projector =
                scope.ServiceProvider
                    .GetKeyedService<IEventProjector>(
                        message.Type);

            await messageProcessor.ProcessAsync(
                message,
                projector,
                cancellationToken);
        }
    }
}