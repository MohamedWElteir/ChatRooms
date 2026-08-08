namespace ChatRooms.Infrastructure.Persistence.Outbox;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);
}