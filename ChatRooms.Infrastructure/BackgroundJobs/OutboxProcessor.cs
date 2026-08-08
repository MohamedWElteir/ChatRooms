using ChatRooms.Infrastructure.BackgroundJobs;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Options;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    OutboxBatchProcessor batchProcessor,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> options) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    private readonly string _workerId =
        $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

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
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var outboxRepository =
            scope.ServiceProvider
                .GetRequiredService<IOutboxRepository>();

        var writeDbContext =
            scope.ServiceProvider
                .GetRequiredService<WriteDbContext>();

        var messages = await outboxRepository.ClaimBatchAsync(
            _options.BatchSize,
            _workerId,
            TimeSpan.FromMinutes(
                _options.ProcessingLeaseDurationMinutes),
            cancellationToken);

        if (messages.Count == 0)
            return;

        await batchProcessor.ProcessBatchAsync(
            messages,
            outboxRepository,
            _workerId,
            messageType =>
                scope.ServiceProvider
                    .GetKeyedService<IEventProjector>(messageType),
            cancellationToken);

        await writeDbContext.SaveChangesAsync(
            cancellationToken);
    }
}