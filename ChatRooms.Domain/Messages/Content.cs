using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRooms.Domain.Messages
{
    public readonly record struct Content
    {
        public string Value { get; }
        private const int MIN_LENGTH = 0;

        private Content(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(value));
            if (value.Length == MIN_LENGTH)
                throw new ArgumentException($"Content must be greater than zero.", nameof(value));
            Value = value;
        }

        public static Content Create(string value) => new(value);
        public override string ToString() => Value;
    }
}
