using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRooms.Domain.Messages.Events
{
    public sealed record MessageCreatedDomainEvent( MessageId MessageId,Description description, RoomId RoomId, DateTime CreatedAt) : DomainEvent(CreatedAt);
}
