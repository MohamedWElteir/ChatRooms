using ChatRooms.Domain.Messages.Events;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace ChatRooms.Domain.Messages
{
    public sealed class Message : Entity<MessageId>
    {
        public RoomId RoomId { get; private set; }
        public Content Content { get; private set; }
        //to-do user attach to message
        private Message(MessageId id, Content content,RoomId roomid, DateTime createdAt) : base(id, createdAt)
        {
            Id = id;
            Content = content;
            RoomId = roomid;
        }

        public static Message Create(Content content,RoomId roomId, DateTime createdAt)
        {
            var message = new Message(MessageId.New(), content,roomId, createdAt);

            return message;

        }
       
    }

}
