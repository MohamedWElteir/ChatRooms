using ChatRooms.Application.Abstractions.Time;

namespace ChatRooms.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
