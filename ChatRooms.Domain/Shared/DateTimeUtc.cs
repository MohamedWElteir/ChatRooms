namespace ChatRooms.Domain.Shared;

/// <summary>
/// Value object representing an absolute UTC timestamp.
/// Internally uses DateTimeOffset.
/// </summary>
public readonly record struct DateTimeUtc
{
    public DateTimeOffset Value { get; }

    /// <summary>
    /// Convenience conversion to DateTime. The returned value is always in UTC.
    /// </summary>
    public DateTime DateTime => Value.UtcDateTime;

    /// <summary>
    /// Unix timestamp (stable for comparisons / persistence).
    /// </summary>
    public long UnixMilliseconds => Value.ToUnixTimeMilliseconds();
    private DateTimeUtc(DateTimeOffset value)
    {
        if (value.Offset != TimeSpan.Zero)
            throw new ArgumentException("DateTimeOffset must be UTC.", nameof(value));

        Value = value;
    }
    public static DateTimeKind Kind => DateTimeKind.Utc;

    /// <summary>
    /// Factory for current UTC time.
    /// </summary>
    public static DateTimeUtc NowUtc() => new(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new DateTimeUtc object from the UTC datetime.
    /// </summary>
    public static DateTimeUtc FromUtc(DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be UTC.", nameof(utc));

        return new DateTimeUtc(new DateTimeOffset(utc));
    }

    public static DateTimeUtc FromLocal(DateTime local)
    {
        if (local.Kind != DateTimeKind.Local)
            throw new ArgumentException("DateTime must be local.", nameof(local));
        return FromUtc(local.ToUniversalTime());
    }

    /// <summary>
    /// From from Unix timestamp.
    /// </summary>
    public static DateTimeUtc FromUnixMilliseconds(long ms) => new(DateTimeOffset.FromUnixTimeMilliseconds(ms));

    public DateTimeUtc AddHours(double hours) => new(Value.AddHours(hours));
    public DateTimeUtc Add(TimeSpan duration) => new(Value.Add(duration));
    public DateTimeUtc AddMinutes(int minutes) => new(Value.AddMinutes(minutes));

    public override string ToString() => Value.UtcDateTime.ToString("o");

    public static bool operator <(DateTimeUtc left, DateTimeUtc right) => left.Value < right.Value;
    public static bool operator >(DateTimeUtc left, DateTimeUtc right) => left.Value > right.Value;
    public static bool operator <=(DateTimeUtc left, DateTimeUtc right) => left.Value <= right.Value;
    public static bool operator >=(DateTimeUtc left, DateTimeUtc right) => left.Value >= right.Value;
}
