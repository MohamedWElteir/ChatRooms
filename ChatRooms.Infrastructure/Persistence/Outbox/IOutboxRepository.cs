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
    /// owned by <paramref name="workerId"/> and their lease has not yet expired.
    /// An expired lease is never renewed (the message must be reclaimed instead).
    /// Returns the number of messages whose lease was renewed. A result lower
    /// than <see cref="messageIds"/> count means the worker lost ownership and
    /// must stop processing the batch.
    /// </summary>
    Task<int> RenewLeaseAsync(
        IReadOnlyList<Guid> messageIds,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);
}