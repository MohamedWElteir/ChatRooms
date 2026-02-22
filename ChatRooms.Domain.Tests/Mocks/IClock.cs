using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Tests.Mocks;

public interface IClock
{
    DateTimeUtc Now { get; }
}

