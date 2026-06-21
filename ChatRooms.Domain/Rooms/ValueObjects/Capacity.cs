namespace ChatRooms.Domain.Rooms.ValueObjects;

public readonly record struct Capacity
{
    public int Value { get; }
    public const int Min = 1;
    public const int Max = 100;

    private Capacity(int value)
    {
        if (value < Min)
            throw new ArgumentException($"Capacity must be at least {Min}.", nameof(value));
        if (value > Max)
            throw new ArgumentException($"Capacity cannot exceed {Max}.", nameof(value));
        Value = value;
    }

    public static Capacity From(int value) => new(value);
    public static implicit operator int(Capacity capacity) => capacity.Value;
    public static implicit operator Capacity(int capacity) => From(capacity);
}