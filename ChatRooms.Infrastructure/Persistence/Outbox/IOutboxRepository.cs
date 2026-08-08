namespace ChatRooms.Infrastructure.Persistence.Outbox;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Extends the lease of the given messages if (and only if) they are still
    /// owned by <paramref name="workerId"/>. Returns the number of messages whose
    /// lease was renewed. A result lower than <see cref="messageIds"/> count means
    /// the worker lost ownership and must stop processing the batch.
    /// </summary>
    Task<int> RenewLeaseAsync(
        IReadOnlyList<Guid> messageIds,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);
}