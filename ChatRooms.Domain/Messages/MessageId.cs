using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRooms.Domain.Messages
{
    public readonly record struct MessageId
    {
        public Guid Value { get; }

        private MessageId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("MessageId cannot be empty.");

            Value = value;
        }

        public static MessageId New() => new(Guid.NewGuid());

        public override string ToString() => Value.ToString();
        public static implicit operator Guid(MessageId MessageId) => MessageId.Value;
    }
}
