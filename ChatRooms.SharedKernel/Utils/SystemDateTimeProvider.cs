using ChatRooms.Domain.Shared;

namespace ChatRooms.SharedKernel.Utils;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeUtc UtcNow => DateTimeUtc.NowUtc();
}
