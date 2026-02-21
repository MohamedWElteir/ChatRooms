using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(
    UserId UserId,
    Name Name,
    Email Email,
    Gender Gender,
    BirthDate BirthDate,
    DateTimeUtc OccurredAt
) : DomainEvent(OccurredAt);
