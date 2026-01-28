using ChatRooms.Domain.Shared;
using ChatRooms.SharedKernel.Utils;

namespace ChatRooms.Domain.Rooms.Events;

public sealed record RoomCreatedDomainEvent(RoomId RoomId, Name Name, RoomCode Code, Capacity Capacity, DateTime DateTime) : DomainEvent(DateTime);