using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRooms.Domain.Messages
{
    public readonly record struct Description
    {
        public string Value { get; }
        private const int MIN_LENGTH = 0;

        private Description(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Description cannot be null or whitespace.", nameof(value));
            if (value.Length == MIN_LENGTH)
                throw new ArgumentException($"Description must be greater than zero.", nameof(value));
            Value = value;
        }

        public static Description Create(string value) => new(value);
        public override string ToString() => Value;
    }
}
