namespace ChatRooms.Domain.Rooms;

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

    public static RoomCode New()
    {
        var value = RoomCodeGenerator.Generate();
        return new RoomCode(value);
    }

    public override string ToString() => Value;

}
