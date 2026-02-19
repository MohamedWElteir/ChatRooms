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
}
