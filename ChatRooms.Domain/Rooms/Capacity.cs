namespace ChatRooms.Domain.Rooms;

public readonly record struct Capacity
{
    public int Value { get; }
    private const int MAX_CAPACITY = 100;
    private Capacity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(value));
        if (value > MAX_CAPACITY)
            throw new ArgumentException($"Capacity cannot exceed {MAX_CAPACITY}.", nameof(value));
        Value = value;
    }
    public static Capacity Create(int value) => new(value);
    public override string ToString() => Value.ToString();

}
