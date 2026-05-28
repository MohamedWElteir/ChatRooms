namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct Name
{
    public string Value { get; }
    private const int MaxLength = 15;

    private Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name cannot be null or empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"Name cannot exceed {MaxLength} characters.", nameof(value));
        if (value.Any(char.IsWhiteSpace))
            throw new ArgumentException("Name cannot contain whitespace.", nameof(value));

        Value = value;
    }

    public static Name From(string value) => new(value);
    public static implicit operator string(Name name) => name.Value;
}