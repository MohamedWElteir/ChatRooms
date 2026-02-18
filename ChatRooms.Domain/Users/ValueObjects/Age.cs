namespace ChatRooms.Domain.Users.ValueObjects;

public readonly record struct Age(int Years, int Months, int Days)
{
    public override string ToString() => $"{Years} years, {Months} months and {Days} days old";
}
