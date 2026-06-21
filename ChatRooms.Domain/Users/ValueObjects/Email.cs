using System.Text.RegularExpressions;

namespace ChatRooms.Domain.Users.ValueObjects;

public readonly partial record struct Email
{
    public string Value { get; }
    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "Email cannot be null or empty.");
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format.", nameof(value));
        Value = value;
    }
    public static Email From(string value) => new(value);
    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => From(email);
    private static bool IsValidEmail(string email)
    {
        return email.Length <= 254 && // RFC 5321 max length
               GenerateEmailRegex().IsMatch(email);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex GenerateEmailRegex();
}