using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Tests.Mocks
{
    public sealed record UnsupportedDomainEvent(DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);
}