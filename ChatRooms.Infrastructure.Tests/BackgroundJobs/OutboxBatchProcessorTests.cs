using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.BackgroundJobs;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChatRooms.Infrastructure.Tests.BackgroundJobs;

public class OutboxBatchProcessorTests
{
    private sealed class FakeOutboxMessageProcessor : IOutboxMessageProcessor
    {
        public List<OutboxMessage> Processed { get; } = [];

        public Task ProcessAsync(
            OutboxMessage message,
            IEventProjector? projector,
            CancellationToken cancellationToken)
        {
            Processed.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProjector : IEventProjector
    {
        public Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    /// <summary>
    /// Simulates the PostgreSQL renewal: returns <paramref name="renewedCount"/>
    /// for the first renewal and <paramref name="subsequentRenewedCount"/> for all
    /// later ones. A value lower than requested signals the worker lost the lease.
    /// </summary>
    private sealed class SequenceFakeOutboxRepository(
        int firstRenewal,
        int subsequentRenewals) : IOutboxRepository
    {
        private int _renewalCalls;

        public int RenewCalls => _renewalCalls;
        public List<Guid> LastRenewedIds { get; private set; } = [];

        public Task<List<OutboxMessage>> ClaimBatchAsync(
            int batchSize,
            string workerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
            => Task.FromResult(new List<OutboxMessage>());

        public Task<int> RenewLeaseAsync(
            IReadOnlyList<Guid> messageIds,
            string workerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref _renewalCalls);
            LastRenewedIds = messageIds.ToList();

            // The SQL renewal can never exceed the number of requested ids; cap
            // the value so the sequence reads as "3/3 renewed, then 1/2".
            var renewal = call == 1 ? firstRenewal : subsequentRenewals;
            return Task.FromResult(Math.Min(messageIds.Count, renewal));
        }
    }

    /// <summary>
    /// Simulates a message whose lease has already expired: even though the
    /// worker still owns it (<c>ProcessingBy</c> matches), the renewal guard
    /// (<c>ProcessingLeaseUntil &gt; now</c>) rejects it with zero rows.
    /// </summary>
    private sealed class ExpiredLeaseOutboxRepository : IOutboxRepository
    {
        public Task<List<OutboxMessage>> ClaimBatchAsync(
            int batchSize,
            string workerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
            => Task.FromResult(new List<OutboxMessage>());

        public Task<int> RenewLeaseAsync(
            IReadOnlyList<Guid> messageIds,
            string workerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
            => Task.FromResult(0);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenLeaseExpiredForSameWorker_AbandonsBatch()
    {
        var messageProcessor = new FakeOutboxMessageProcessor();
        var sut = CreateProcessor(messageProcessor);
        var repo = new ExpiredLeaseOutboxRepository();
        var messages = new[]
        {
            CreateMessage(),
            CreateMessage(),
            CreateMessage()
        };

        await sut.ProcessBatchAsync(
            messages, repo, "worker-1", _ => new FakeProjector(), CancellationToken.None);

        Assert.Empty(messageProcessor.Processed);
    }

    private static OutboxBatchProcessor CreateProcessor(FakeOutboxMessageProcessor messageProcessor) => new(
        messageProcessor,
        Microsoft.Extensions.Options.Options.Create(new Options.OutboxOptions()),
        NullLogger<OutboxBatchProcessor>.Instance);

    private static OutboxMessage CreateMessage() =>
        OutboxMessage.Create("TestEvent", "{}", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(-1)));

    [Fact]
    public async Task ProcessBatchAsync_WhenEveryRenewalSucceeds_ProcessesAllMessages()
    {
        var messageProcessor = new FakeOutboxMessageProcessor();
        var sut = CreateProcessor(messageProcessor);
        var repo = new SequenceFakeOutboxRepository(firstRenewal: 3, subsequentRenewals: 2);
        var messages = new[]
        {
            CreateMessage(),
            CreateMessage(),
            CreateMessage()
        };

        await sut.ProcessBatchAsync(
            messages, repo, "worker-1", _ => new FakeProjector(), CancellationToken.None);

        Assert.Equal(3, messageProcessor.Processed.Count);
        Assert.Equal(3, repo.RenewCalls);
        Assert.Equal([messages[2].Id], repo.LastRenewedIds);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenRenewalReturnsLessThanRequested_AbandonsBatch()
    {
        var messageProcessor = new FakeOutboxMessageProcessor();
        var sut = CreateProcessor(messageProcessor);

        // First renewal renews 3/3 (message 0 processed), second renewal only
        // 1/2 -> the worker lost ownership of one message and must stop.
        var repo = new SequenceFakeOutboxRepository(firstRenewal: 3, subsequentRenewals: 1);
        var messages = new[]
        {
            CreateMessage(),
            CreateMessage(),
            CreateMessage()
        };

        await sut.ProcessBatchAsync(
            messages, repo, "worker-1", _ => new FakeProjector(), CancellationToken.None);

        Assert.Single(messageProcessor.Processed);
        Assert.Equal(2, repo.RenewCalls);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenFirstRenewalFails_ProcessesNothing()
    {
        var messageProcessor = new FakeOutboxMessageProcessor();
        var sut = CreateProcessor(messageProcessor);
        var repo = new SequenceFakeOutboxRepository(firstRenewal: 2, subsequentRenewals: 0);
        var messages = new[]
        {
            CreateMessage(),
            CreateMessage(),
            CreateMessage()
        };

        await sut.ProcessBatchAsync(
            messages, repo, "worker-1", _ => new FakeProjector(), CancellationToken.None);

        Assert.Empty(messageProcessor.Processed);
        Assert.Equal(1, repo.RenewCalls);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithNoMessages_DoesNothing()
    {
        var messageProcessor = new FakeOutboxMessageProcessor();
        var sut = CreateProcessor(messageProcessor);
        var repo = new SequenceFakeOutboxRepository(firstRenewal: 3, subsequentRenewals: 3);

        await sut.ProcessBatchAsync(
            [], repo, "worker-1", _ => new FakeProjector(), CancellationToken.None);

        Assert.Empty(messageProcessor.Processed);
        Assert.Equal(0, repo.RenewCalls);
    }
}