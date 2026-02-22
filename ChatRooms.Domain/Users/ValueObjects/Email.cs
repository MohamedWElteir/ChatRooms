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
    private static readonly Regex EmailRegex = GenerateEmailRegex();
    private static bool IsValidEmail(string email)
    {
        if (email.Length > 254) return false; // RFC 5321 max length
        return EmailRegex.IsMatch(email);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex GenerateEmailRegex();
}