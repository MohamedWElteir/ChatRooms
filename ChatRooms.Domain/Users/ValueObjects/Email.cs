namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct Email
{
    public string Value { get; }
    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format.", nameof(value));
        Value = value;
    }
    public static Email From(string value) => new (value);
    public override string ToString() => Value;
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}