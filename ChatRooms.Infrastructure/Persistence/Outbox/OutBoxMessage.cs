using ChatRooms.Domain.Shared;

namespace ChatRooms.Infrastructure.Persistence.Outbox;

public sealed record OutboxMessage(
    Guid Id,
    string Type,
    string Content,
    string? ErrorMessage,
    DateTimeUtc OccurredOn,
    DateTimeUtc? ProcessedOn,
    int RetryCount,
    bool IsProcessed
    );
