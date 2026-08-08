using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.BackgroundJobs;
using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChatRooms.Infrastructure.Tests.BackgroundJobs;

public class OutboxMessageProcessorTests
{
    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class ThrowingProjector : IEventProjector
    {
        public Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
            => throw new InvalidOperationException("boom");
    }

    private sealed class SuccessProjector : IEventProjector
    {
        public Task ProjectAsync(string eventContent, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static OutboxMessageProcessor CreateSut(int maxRetryCount = 5) => new(
        new FixedDateTimeProvider(DateTime.UtcNow),
        Microsoft.Extensions.Options.Options.Create(
            new ChatRooms.Infrastructure.Options.OutboxOptions { MaxRetryCount = maxRetryCount }),
        NullLogger<OutboxMessageProcessor>.Instance);

    private static OutboxMessage CreateMessage(int retryCount = 0)
    {
        var message = OutboxMessage.Create(
            "TestEvent", "{}", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(-1)));
        for (var i = 0; i < retryCount; i++)
            message.RecordFailure("previous failure");
        return message;
    }

    [Fact]
    public async Task ProcessAsync_WhenProjectorSucceeds_MarksAsProcessed()
    {
        var sut = CreateSut();
        var message = CreateMessage();

        await sut.ProcessAsync(message, new SuccessProjector(), CancellationToken.None);

        Assert.True(message.IsProcessed);
        Assert.False(message.IsDeadLetter);
        Assert.NotNull(message.ProcessedOn);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
        Assert.Equal(0, message.RetryCount);
    }

    [Fact]
    public async Task ProcessAsync_WhenProjectorThrows_RecordsFailureAndReleasesLease()
    {
        var sut = CreateSut();
        var message = CreateMessage();
        message.Claim("worker-1", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(2)));

        await sut.ProcessAsync(message, new ThrowingProjector(), CancellationToken.None);

        Assert.False(message.IsProcessed);
        Assert.False(message.IsDeadLetter);
        Assert.Equal(1, message.RetryCount);
        Assert.NotNull(message.ErrorMessage);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
    }

    [Fact]
    public async Task ProcessAsync_WhenMaxRetriesReached_MovesToDeadLetter()
    {
        var sut = CreateSut(maxRetryCount: 3);
        var message = CreateMessage(retryCount: 3);

        await sut.ProcessAsync(message, new ThrowingProjector(), CancellationToken.None);

        Assert.False(message.IsProcessed);
        Assert.True(message.IsDeadLetter);
        Assert.StartsWith("[DEAD LETTER]", message.ErrorMessage);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
    }

    [Fact]
    public async Task ProcessAsync_WhenNoProjector_MovesToDeadLetter()
    {
        var sut = CreateSut();
        var message = CreateMessage();

        await sut.ProcessAsync(message, null, CancellationToken.None);

        Assert.True(message.IsDeadLetter);
        Assert.False(message.IsProcessed);
        Assert.NotNull(message.ProcessedOn);
        Assert.Contains("No projector", message.ErrorMessage);
    }

    [Fact]
    public void RecordFailure_ShouldIncrementRetryCountAndCaptureError()
    {
        var message = CreateMessage();
        message.Claim("worker-1", DateTimeUtc.FromUtc(DateTime.UtcNow.AddMinutes(2)));

        message.RecordFailure("network error");

        Assert.Equal(1, message.RetryCount);
        Assert.Equal("network error", message.ErrorMessage);
        Assert.Null(message.ProcessingBy);
        Assert.Null(message.ProcessingLeaseUntil);
    }
}