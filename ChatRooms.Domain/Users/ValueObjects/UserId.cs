namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct UserId
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.");

        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
    public static implicit operator Guid(UserId userId) => userId.Value;
}
