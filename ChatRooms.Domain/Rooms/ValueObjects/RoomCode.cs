namespace ChatRooms.Domain.Rooms.ValueObjects;

public readonly record struct RoomCode
{
    public readonly string Value { get; }

    private RoomCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RoomCode cannot be null or whitespace.", nameof(value));
        if (value.Length != 8)
            throw new ArgumentException("RoomCode must be exactly 8 characters long.", nameof(value));
        Value = value;
    }

    public static RoomCode From(string value) => new(value);
    public static implicit operator string(RoomCode code) => code.Value;

}
