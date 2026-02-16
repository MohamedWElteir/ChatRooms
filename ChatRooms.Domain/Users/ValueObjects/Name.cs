namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct Name
{
    public string Value { get; }
    private const int MAX_LENGTH = 15;
    private Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name cannot be null or empty.", nameof(value));
        if (value.Length > MAX_LENGTH)
            throw new ArgumentException($"Name cannot exceed {MAX_LENGTH} characters.", nameof(value));
        if (value.Any(char.IsWhiteSpace))
            throw new ArgumentException("Name cannot contain whitespace.", nameof(value));

        Value = value;
    }
    public static Name From(string value) => new (value);
    public override string ToString() => Value;
}
