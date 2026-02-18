using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users.Events;

public sealed record UserDeletedDomainEvent(UserId UserId, DeletionReason Reason) : DomainEvent;
