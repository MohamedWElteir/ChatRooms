namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct BirthDate
{
    public DateTime Value { get; }

    private BirthDate(DateTime value)
    {
        if (value > DateTime.UtcNow)
            throw new ArgumentException("BirthDate cannot be in the future.", nameof(value));

        Value = DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    }

    /// <summary>
    /// Factory to create a new BirthDate object.
    /// </summary>
    /// <param name="value">The DateTime.</param>
    /// <returns>
    /// A new BirthDate object from the provided Date.
    /// </returns>
    public static BirthDate From(DateTime value) => new(value);

    /// <summary>
    /// Function to get the age based on the BirthDate.
    /// </summary>
    /// <param name="from"></param>
    /// <returns>
    ///  Returns the age based of the BirthDate. If no value is passed, it returns the age from the current date.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// If the date is less than the BirthDate.
    /// </exception>
    public Age CalculateAge(DateTime? from = null)
    {
        var effectiveDate = (from ?? DateTime.UtcNow).Date;

        if (effectiveDate < Value)
            throw new InvalidOperationException("Invalid age calculation.");

        int years = effectiveDate.Year - Value.Year;
        int months = effectiveDate.Month - Value.Month;
        int days = effectiveDate.Day - Value.Day;

        if (days < 0)
        {
            months--;

            var previousMonth = effectiveDate.AddMonths(-1);
            days += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        }

        if (months < 0)
        {
            years--;
            months += 12;
        }

        return new Age(years, months, days);
    }

    public override string ToString() => Value.ToShortDateString();
    public static implicit operator DateTime(BirthDate birthDate) => birthDate.Value;
    public static implicit operator BirthDate(DateTime dateTime) => From(dateTime);
}
