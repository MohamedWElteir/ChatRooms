namespace ChatRooms.Domain.Shared;

/// <summary>
/// Value object representing a UTC DateTime.
/// Ensures that all domain timestamps are always in UTC.
/// </summary>
public readonly record struct DateTimeUtc
{
    public DateTime Value { get; }

    public DateTimeUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be UTC.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Implicitly converts to DateTime for convenience.
    /// </summary>
    public static implicit operator DateTime(DateTimeUtc dt) => dt.Value;

    /// <summary>
    /// Factory method to create a DateTimeUtc from local time (converted to UTC).
    /// </summary>
    public static DateTimeUtc FromLocal(DateTime localTime) => new(localTime.ToUniversalTime());

    /// <summary>
    /// Factory method to create a DateTimeUtc representing the current UTC time.
    /// </summary>
    public static DateTimeUtc NowUtc() => new(DateTime.UtcNow);

    /// <summary>
    /// Method to add hours to the UTC DateTime.
    /// </summary>
    /// <param name="hours"></param>
    /// <returns>
    /// New DateTimeUtc instance with the added hours.
    /// </returns>
    public DateTimeUtc AddHours(double hours) => new (Value.AddHours(hours));
    public override string ToString() => Value.ToString("o"); // ISO 8601 format

    public DateTimeUtc AddMinutes(int minutes) => new (Value.AddMinutes(minutes));
}
