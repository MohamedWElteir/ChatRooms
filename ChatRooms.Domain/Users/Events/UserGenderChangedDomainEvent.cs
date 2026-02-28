using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users.Events;

public sealed record UserGenderChangedDomainEvent(UserId UserId, Gender NewGender, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);