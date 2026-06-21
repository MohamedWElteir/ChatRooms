namespace ChatRooms.Domain.Rooms.ValueObjects;

public readonly record struct Name
{
    public string Value { get; }
    public const int MaxLength = 50;

    private Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"Name cannot exceed {MaxLength} characters.", nameof(value));
        if (!char.IsLetter(value[0]))
            throw new ArgumentException("Name must start with a letter.", nameof(value));

        Value = value;
    }

    public static Name From(string value) => new(value);
    public static implicit operator string(Name name) => name.Value;
    public static implicit operator Name(string name) => From(name);
}
