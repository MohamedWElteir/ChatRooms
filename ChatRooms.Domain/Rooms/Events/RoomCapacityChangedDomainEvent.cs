using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;

namespace ChatRooms.Domain.Rooms.Events;
public sealed record RoomCapacityChangedDomainEvent(RoomId RoomId, Capacity NewCapacity, DateTimeUtc OccurredAt) : DomainEvent(OccurredAt);