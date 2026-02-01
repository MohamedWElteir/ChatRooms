using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(
    UserId UserId,
    Name Name
) : DomainEvent;
