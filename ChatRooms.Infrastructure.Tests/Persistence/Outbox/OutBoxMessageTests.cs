using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.Persistence.Outbox;

namespace ChatRooms.Infrastructure.Tests.Persistence.Outbox;

public class OutBoxMessageTests
{
    private static OutboxMessage CreateMessage() =>
        OutboxMessage.Create("TestEvent", "{}", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(-1)));

    [Fact]
    public void Create_ShouldBeUnprocessedAndUnclaimed()
    {
        var message = CreateMessage();

        Assert.False(message.IsProcessed);
        Assert.False(message.IsDeadLetter);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
        Assert.Null(message.ErrorMessage);
    }

    [Fact]
    public void Claim_ShouldSetWorkerAndLease()
    {
        var message = CreateMessage();
        var leaseUntil = DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(2));

        message.Claim("worker-1", leaseUntil);

        Assert.Equal("worker-1", message.ProcessingBy);
        Assert.Equal(leaseUntil.DateTime, message.ProcessingLeaseUntil?.DateTime);
    }

    [Fact]
    public void MarkAsProcessed_ShouldClearOwnerAndLease_AndClearError()
    {
        var message = CreateMessage();
        message.Claim("worker-1", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(2)));
        message.RecordFailure("stale failure");

        message.MarkAsProcessed(DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.True(message.IsProcessed);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
        Assert.Null(message.ErrorMessage);
        Assert.NotNull(message.ProcessedOn);
    }

    [Fact]
    public void MarkAsDeadLetter_ShouldSetFlag_KeepNotProcessed_ClearOwner()
    {
        var message = CreateMessage();
        message.Claim("worker-1", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(2)));

        message.MarkAsDeadLetter(DateTimeUtc.FromUtc(DateTime.UtcNow), "fatal");

        Assert.True(message.IsDeadLetter);
        Assert.False(message.IsProcessed);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
        Assert.NotNull(message.ProcessedOn);
        Assert.Contains("[DEAD LETTER]", message.ErrorMessage);
    }

    [Fact]
    public void RecordFailure_ShouldReleaseLeaseAndIncrementRetryCount()
    {
        var message = CreateMessage();
        message.Claim("worker-1", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(2)));

        message.RecordFailure("retryable");

        Assert.Equal(1, message.RetryCount);
        Assert.Equal("retryable", message.ErrorMessage);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
        Assert.False(message.IsProcessed);
        Assert.False(message.IsDeadLetter);
    }
}