namespace ChatRooms.SharedKernel.Utils;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
