using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.Outbox;

public sealed class OutboxRepository(
    WriteDbContext dbContext) : IOutboxRepository
{
    public async Task<List<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        var now = DateTimeUtc.FromUtc(DateTime.UtcNow);
        var leaseUntil =
            DateTimeUtc.FromUtc(DateTime.UtcNow.Add(leaseDuration));

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var messages = await dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                                  SELECT *
                                  FROM "OutboxMessages"
                                  WHERE "IsProcessed" = false
                                    AND "IsDeadLetter" = false
                                    AND (
                                          "ProcessingLeaseUntil" IS NULL
                                          OR "ProcessingLeaseUntil" < {now}
                                        )
                                  ORDER BY "OccurredOn"
                                  FOR UPDATE SKIP LOCKED
                                  LIMIT {batchSize}
                                  """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Claim(workerId, leaseUntil);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages;
    }

    public async Task<int> RenewLeaseAsync(
        IReadOnlyList<Guid> messageIds,
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        if (messageIds.Count == 0)
            return 0;

        var leaseUntil =
            DateTimeUtc.FromUtc(DateTime.UtcNow.Add(leaseDuration));

        var ids = messageIds.ToArray();

        return await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE "OutboxMessages"
             SET "ProcessingLeaseUntil" = {leaseUntil}
             WHERE "Id" = ANY({ids})
               AND "ProcessingBy" = {workerId}
               AND "ProcessingLeaseUntil" IS NOT NULL
               AND "IsProcessed" = false
               AND "IsDeadLetter" = false
             """,
            cancellationToken);
    }
}