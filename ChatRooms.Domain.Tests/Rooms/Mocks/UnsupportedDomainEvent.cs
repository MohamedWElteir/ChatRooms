using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Tests.Rooms.Mocks
{
    public sealed record UnsupportedDomainEvent(DateTime OccuredAt) : DomainEvent(OccuredAt);
}