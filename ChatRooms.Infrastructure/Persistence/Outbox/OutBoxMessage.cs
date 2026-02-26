using ChatRooms.Domain.Shared;

namespace ChatRooms.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeUtc OccurredOn { get; init; }

    public string? ErrorMessage { get; private set; }
    public DateTimeUtc? ProcessedOn { get; private set; }
    public int RetryCount { get; private set; }
    public bool IsProcessed { get; private set; }
    public bool IsDeadLetter { get; private set; }

    private OutboxMessage() { }

    public void MarkAsProcessed(DateTimeUtc processedOn)
    {
        ProcessedOn = processedOn;
        IsProcessed = true;
        ErrorMessage = null;
    }

    public void RecordFailure(string error)
    {
        ErrorMessage = error;
        RetryCount++;
    }
    public void MarkAsDeadLetter(DateTimeUtc deadLetteredOn, string finalError)
    {
        ProcessedOn = deadLetteredOn;
        IsProcessed = false;
        IsDeadLetter = true;
        ErrorMessage = $"[DEAD LETTER] {finalError}";
    }
}