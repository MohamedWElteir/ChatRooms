namespace ChatRooms.Domain.Rooms.ValueObjects;

public readonly record struct RoomId
{
    public Guid Value { get; }

    private RoomId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoomId cannot be empty.");

        Value = value;
    }

    public static RoomId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static RoomId From(Guid value) => new(value);
    public static implicit operator RoomId(Guid value) => new(value);
    public static implicit operator Guid(RoomId roomId) => roomId.Value;
}
