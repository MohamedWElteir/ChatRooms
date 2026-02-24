using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users.Events;

public sealed record UserEmailChangedDomainEvent(UserId UserId, Email NewEmail, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);