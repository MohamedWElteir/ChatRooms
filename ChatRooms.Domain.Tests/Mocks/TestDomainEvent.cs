using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Tests.Mocks;

public sealed record TestDomainEvent(DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);