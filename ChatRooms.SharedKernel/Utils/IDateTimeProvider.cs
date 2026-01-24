namespace ChatRooms.SharedKernel.Utils;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
