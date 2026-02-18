namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct BirthDate
{
    public DateTime Value { get; }

    private BirthDate(DateTime value)
    {
        if (value > DateTime.UtcNow)
            throw new ArgumentException("BirthDate cannot be in the future.", nameof(value));

        Value = value.Date;
    }

    public static BirthDate From(DateTime value) => new(value);

    public Age AgeAt(DateTime today)
    {
        today = today.Date;

        if (today < Value)
            throw new InvalidOperationException("Invalid age calculation.");

        int years = today.Year - Value.Year;
        int months = today.Month - Value.Month;
        int days = today.Day - Value.Day;

        if (days < 0)
        {
            months--;

            var previousMonth = today.AddMonths(-1);
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
}
