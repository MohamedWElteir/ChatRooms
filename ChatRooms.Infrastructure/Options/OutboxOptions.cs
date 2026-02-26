namespace ChatRooms.Infrastructure.Options;

public sealed class OutboxOptions
{
    public const string SectionName = nameof(OutboxOptions);

    public int MaxRetryCount { get; init; } = 5;
    public int BatchSize { get; init; } = 20;
    public int PollingIntervalSeconds { get; init; } = 3;
}
