using ChatRooms.Domain.Shared;

namespace ChatRooms.SharedKernel.Utils;

public interface IDateTimeProvider
{
    DateTimeUtc UtcNow { get; }
}
