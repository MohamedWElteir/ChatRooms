using ChatRooms.Domain.Messages;
using ChatRooms.Domain.Messages.Events;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Events;
namespace ChatRooms.Domain.Tests.Messages
{
    public sealed class MessageTests
    {
        private Room room;
        public MessageTests() {

            var name = Name.Create("GeneralChat");
            var capacity = Capacity.Create(100);
            var createdAt = DateTime.UtcNow;
            room = Room.Create(name, capacity, createdAt);

        }
        [Fact]
        public void CreateMessage_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var description = Description.Create("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin tincidunt elit ac nibh finibus, id elementum nulla tincidunt. Mauris ut tortor quis magna volutpat sagittis vel pellentesque mi. Sed id euismod nibh. Duis suscipit bibendum dictum. In mattis ac enim non blandit. Aliquam ut consequat arcu. Cras blandit gravida consequat. Nam in est ullamcorper ipsum malesuada auctor. Nam mi tortor, porttitor ac mollis ut, egestas interdum risus. In vitae lacus lobortis, viverra risus suscipit, consectetur massa. In dapibus, mauris eget malesuada convallis, nisi eros pellentesque justo, non vestibulum mi massa sed arcu. Aliquam suscipit urna a augue tempor posuere. Donec mattis ullamcorper metus eu dignissim. Pellentesque maximus urna ut metus blandit, eget rutrum tellus porta. Nunc ipsum dolor, pellentesque in lectus quis, tincidunt consectetur est.");
            var createdAt = DateTime.UtcNow;
            // Act
            var message = Message.Create(description, room.Id, createdAt);
            // Assert
            Assert.Equal(room.Id, message.RoomId);
            Assert.Equal(description, message.Description);
            Assert.Equal(createdAt, message.CreatedAt);
        }
        [Fact]
        public void CreateMessage_ShouldRaiseMessageCreatedDomainEvent()
        {
            // Arrange
            var description = Description.Create("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin tincidunt elit ac nibh finibus, id elementum nulla tincidunt. Mauris ut tortor quis magna volutpat sagittis vel pellentesque mi. Sed id euismod nibh. Duis suscipit bibendum dictum. In mattis ac enim non blandit. Aliquam ut consequat arcu. Cras blandit gravida consequat. Nam in est ullamcorper ipsum malesuada auctor. Nam mi tortor, porttitor ac mollis ut, egestas interdum risus. In vitae lacus lobortis, viverra risus suscipit, consectetur massa. In dapibus, mauris eget malesuada convallis, nisi eros pellentesque justo, non vestibulum mi massa sed arcu. Aliquam suscipit urna a augue tempor posuere. Donec mattis ullamcorper metus eu dignissim. Pellentesque maximus urna ut metus blandit, eget rutrum tellus porta. Nunc ipsum dolor, pellentesque in lectus quis, tincidunt consectetur est.");
            var createdAt = DateTime.UtcNow;
            // Act
            var message = Message.Create(description, room.Id, createdAt);
            // Assert
            var domainEvents = message.DomainEvents;
            Assert.Single(domainEvents);
            var meesageCreatedEvent = Assert.IsType<MessageCreatedDomainEvent>(domainEvents.First());
            Assert.Equal(message.Id, meesageCreatedEvent.MessageId);
            Assert.Equal(room.Id, message.RoomId);
            Assert.Equal(description, message.Description);
            Assert.Equal(createdAt, message.CreatedAt);
        }
        [Fact]
        public void MeesageId_New_ShouldGenerateNonEmptyGuid()
        {
            // Act
            var messageId = MessageId.New();
            // Assert
            Assert.NotEqual(Guid.Empty, messageId.Value);
        }
        [Fact]
        public void Description_Create_ShouldThrowException_ForInvalidNames()
        {
            // Arrange
            var invalidDescriptions = new[]
            {
            "", // Empty
            "   ", // Whitespace
        };
            // Act & Assert
            foreach (var invalidDescription in invalidDescriptions)
            {
                Assert.Throws<ArgumentException>(() => Description.Create(invalidDescription));
            }
        }
        [Fact]
        public void Message_Equality_ShouldWorkCorrectly()
        {
            // Arrange
            var description = Description.Create("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin tincidunt elit ac nibh finibus, id elementum nulla tincidunt. Mauris ut tortor quis magna volutpat sagittis vel pellentesque mi. Sed id euismod nibh. Duis suscipit bibendum dictum. In mattis ac enim non blandit. Aliquam ut consequat arcu. Cras blandit gravida consequat. Nam in est ullamcorper ipsum malesuada auctor. Nam mi tortor, porttitor ac mollis ut, egestas interdum risus. In vitae lacus lobortis, viverra risus suscipit, consectetur massa. In dapibus, mauris eget malesuada convallis, nisi eros pellentesque justo, non vestibulum mi massa sed arcu. Aliquam suscipit urna a augue tempor posuere. Donec mattis ullamcorper metus eu dignissim. Pellentesque maximus urna ut metus blandit, eget rutrum tellus porta. Nunc ipsum dolor, pellentesque in lectus quis, tincidunt consectetur est.");
            var createdAt = DateTime.UtcNow;

            var message1 = Message.Create(description, room.Id, createdAt);
            var message2 = Message.Create(description, room.Id, createdAt);
            var message3 = message1;
            // Act & Assert
            Assert.NotEqual(message1, message2); // Different instances with different IDs
            Assert.Equal(message1, message3); // Same instance
            Assert.True(message1 == message3);
            Assert.False(message1 != message3);
        }

        [Fact]
        public void Room_DomainEvents_ShouldBeClearedCorrectly()
        {
            // Arrange
            var description = Description.Create("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin tincidunt elit ac nibh finibus, id elementum nulla tincidunt. Mauris ut tortor quis magna volutpat sagittis vel pellentesque mi. Sed id euismod nibh. Duis suscipit bibendum dictum. In mattis ac enim non blandit. Aliquam ut consequat arcu. Cras blandit gravida consequat. Nam in est ullamcorper ipsum malesuada auctor. Nam mi tortor, porttitor ac mollis ut, egestas interdum risus. In vitae lacus lobortis, viverra risus suscipit, consectetur massa. In dapibus, mauris eget malesuada convallis, nisi eros pellentesque justo, non vestibulum mi massa sed arcu. Aliquam suscipit urna a augue tempor posuere. Donec mattis ullamcorper metus eu dignissim. Pellentesque maximus urna ut metus blandit, eget rutrum tellus porta. Nunc ipsum dolor, pellentesque in lectus quis, tincidunt consectetur est.");
            var createdAt = DateTime.UtcNow;
            var message = Message.Create(description, room.Id, createdAt);
            // Act
            message.ClearDomainEvents();
            // Assert
            Assert.Empty(message.DomainEvents);
        }

        [Fact]
        public void MeeageId_ToString_ShouldReturnGuidString()
        {
            // Arrange
            var messageId = MessageId.New();
            // Act
            var messageIdString = messageId.ToString();
            // Assert
            Assert.Equal(messageId.Value.ToString(), messageIdString);
        }
    }
}
