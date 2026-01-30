using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Tests.Rooms.Mocks
{
    public sealed record UnsupportedDomainEvent(DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);
}