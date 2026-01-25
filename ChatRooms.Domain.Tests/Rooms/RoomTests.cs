using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Events;

namespace ChatRooms.Domain.Tests.Rooms;

public sealed class RoomTests
{
    [Fact]
    public void CreateRoom_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var name = Name.Create("GeneralChat");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        // Assert
        Assert.Equal(name, room.Name);
        Assert.Equal(capacity, room.Capacity);
        Assert.Equal(createdAt, room.CreatedAt);
        Assert.NotNull(room.RoomCode.Value);
        Assert.Equal(8, room.RoomCode.Value.Length);
    }

    [Fact]
    public void CreateRoom_ShouldRaiseRoomCreatedDomainEvent()
    {
        // Arrange
        var name = Name.Create("TechTalk");
        var capacity = Capacity.Create(50);
        var createdAt = DateTime.UtcNow;
        // Act
        var room = Room.Create(name, capacity, createdAt);
        // Assert
        var domainEvents = room.DomainEvents;
        Assert.Single(domainEvents);
        var roomCreatedEvent = Assert.IsType<RoomCreatedDomainEvent>(domainEvents.First());
        Assert.Equal(room.Id, roomCreatedEvent.RoomId);
        Assert.Equal(name, roomCreatedEvent.Name);
        Assert.Equal(capacity, roomCreatedEvent.Capacity);
        Assert.Equal(createdAt, roomCreatedEvent.OccurredOn);
    }

    [Fact]
    public void RoomId_New_ShouldGenerateNonEmptyGuid()
    {
        // Act
        var roomId = RoomId.NewID();
        // Assert
        Assert.NotEqual(Guid.Empty, roomId.Value);
    }

    [Fact]
    public void Name_Create_ShouldThrowException_ForInvalidNames()
    {
        // Arrange
        var invalidNames = new[]
        {
            "", // Empty
            "   ", // Whitespace
            new string('A', 51), // Exceeds max length
            "1InvalidStart", // Does not start with a letter
        };
        // Act & Assert
        foreach (var invalidName in invalidNames)
        {
            Assert.Throws<ArgumentException>(() => Name.Create(invalidName));
        }
    }

    [Fact]
    public void Capacity_Create_ShouldThrowException_ForInvalidValues()
    {
        // Arrange
        var invalidCapacities = new[] { 0, -10, 1001 };
        // Act & Assert
        foreach (var invalidCapacity in invalidCapacities)
        {
            Assert.Throws<ArgumentException>(() => Capacity.Create(invalidCapacity));
        }
    }

    [Fact]
    public void Capacity_Create_ShouldInitializeCorrectly_ForValidValues()
    {
        // Arrange
        var validCapacities = new[] { 1, 2, 50, 99, 100 };
        // Act & Assert
        foreach (var validCapacity in validCapacities)
        {
            var capacity = Capacity.Create(validCapacity);
            Assert.Equal(validCapacity, capacity.Value);
        }
    }

    [Fact]
    public void Room_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var name = Name.Create("EqualityTest");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room1 = Room.Create(name, capacity, createdAt);
        var room2 = Room.Create(name, capacity, createdAt);
        var room3 = room1;
        // Act & Assert
        Assert.NotEqual(room1, room2); // Different instances with different IDs
        Assert.Equal(room1, room3); // Same instance
        Assert.True(room1 == room3);
        Assert.False(room1 != room3);
    }

    [Fact]
    public void Room_DomainEvents_ShouldBeClearedCorrectly()
    {
        // Arrange
        var name = Name.Create("EventClearTest");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        // Act
        room.ClearDomainEvents();
        // Assert
        Assert.Empty(room.DomainEvents);
    }

    [Fact]
    public void Room_UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var name = Name.Create("UpdateTest");
        var capacity = Capacity.Create(50);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        var newUpdatedAt = createdAt.AddHours(1);
        // Act
        room.UpdatedAt = newUpdatedAt;
        // Assert
        Assert.Equal(newUpdatedAt, room.UpdatedAt);
    }

    [Fact]
    public void RoomId_ToString_ShouldReturnGuidString()
    {
        // Arrange
        var roomId = RoomId.NewID();
        // Act
        var roomIdString = roomId.ToString();
        // Assert
        Assert.Equal(roomId.Value.ToString(), roomIdString);
    }

    [Fact]
    public void Name_ToString_ShouldReturnNameValue()
    {
        // Arrange
        var nameValue = "TestRoom";
        var name = Name.Create(nameValue);
        // Act
        var nameString = name.ToString();
        // Assert
        Assert.Equal(nameValue, nameString);
    }

    [Fact]
    public void RoomId_ImplicitConversionToGuid_ShouldWorkCorrectly()
    {
        // Arrange
        var roomId = RoomId.NewID();
        // Act
        Guid guidValue = roomId;
        // Assert
        Assert.Equal(roomId.Value, guidValue);
    }

    [Fact]
    public void Capacity_ToString_ShouldReturnValueString()
    {
        // Arrange
        var capacityValue = 75;
        var capacity = Capacity.Create(capacityValue);
        // Act
        var capacityString = capacity.ToString();
        // Assert
        Assert.Equal(capacityValue.ToString(), capacityString);
    }
}
