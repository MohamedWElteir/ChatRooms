using ChatRooms.Domain.Messages.Events;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRooms.Domain.Messages
{
    internal sealed class Message : Entity<MessageId>
    {
        public RoomId RoomId { get; private set; }
        public Description Description { get; private set; }
        //to-do user attach to message
        private Message(MessageId id, Description description,RoomId roomid, DateTime createdAt) : base(id, createdAt)
        {
            RoomId = roomid;
            Description = description;
            Raise(new MessageCreatedDomainEvent(id, description,roomid ,createdAt));

        }

        public static Message Create(Description description,RoomId roomId, DateTime createdAt)
        {
            var message = new Message(MessageId.New(), description,roomId, createdAt);

            return message;

        }
    }

}
