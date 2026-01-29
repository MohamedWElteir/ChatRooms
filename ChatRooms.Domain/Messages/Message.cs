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
    public sealed class Message : AggregateRoot<MessageId>
    {
        public RoomId RoomId { get; private set; }
        public Description Description { get; private set; }
        //to-do user attach to message
        private Message(MessageId id, Description description,RoomId roomid, DateTime createdAt) : base(id, createdAt)
        {
            Raise(new MessageCreatedDomainEvent(id, description,roomid ,createdAt));

        }

        public static Message Create(Description description,RoomId roomId, DateTime createdAt)
        {
            var message = new Message(MessageId.New(), description,roomId, createdAt);

            return message;

        }

        public override void Apply(IDomainEvent @event)
        {
            switch (@event)
            {
                case MessageCreatedDomainEvent e:
                    Apply(e);
                    break;

                default:
                    throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(Message)}");
            }
        }
        private void Apply(MessageCreatedDomainEvent @event)
        {
            Id = @event.MessageId;
            Description = @event.description;
            RoomId = @event.RoomId;
        }
    }

}
