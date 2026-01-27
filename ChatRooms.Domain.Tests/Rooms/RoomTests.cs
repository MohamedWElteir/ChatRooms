using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Enums;
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
        Assert.NotNull(room.Code.Value);
        Assert.Equal(8, room.Code.Value.Length);
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
        Assert.Equal(createdAt, roomCreatedEvent.OccurredAt);
    }

    [Fact]
    public void RoomId_New_ShouldGenerateNonEmptyGuid()
    {
        // Act
        var roomId = RoomId.New();
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
    public void RoomId_ToString_ShouldReturnGuidString()
    {
        // Arrange
        var roomId = RoomId.New();
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
        var roomId = RoomId.New();
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

    [Fact]
    public void Room_Capacity_Change_ShouldChangeWithValidCapacity()
    {
        // Arrange
        var roomName = Name.Create("CapacityChangeRoom");
        var capacityValue = 75;
        var initialCapacity = Capacity.Create(capacityValue);
        var room = Room.Create(roomName, initialCapacity, DateTime.UtcNow);
        var newCapacityValue = 100;
        var newCapacity = Capacity.Create(newCapacityValue);
        // Act
        room.ChangeCapacity(newCapacity);
        // Assert
        Assert.Equal(newCapacityValue, room.Capacity.Value);
    }

    [Fact]
    public void Room_Rename_ShouldRaiseRoomRenamedDomainEvent()
    {
        // Arrange
        var initialName = Name.Create("InitialRoomName");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(initialName, capacity, createdAt);
        var newName = Name.Create("RenamedRoom");
        // Act
        room.Rename(newName);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomRenamedEvent = domainEvents.OfType<RoomRenamedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomRenamedEvent);
        Assert.Equal(room.Id, roomRenamedEvent.RoomId);
        Assert.Equal(newName, roomRenamedEvent.NewName);
    }

    [Fact]
    public void Room_Archive_ShouldRaiseRoomArchivedDomainEvent()
    {
        // Arrange
        var name = Name.Create("ArchiveTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        var archivedAt = DateTime.UtcNow.AddHours(1);
        // Act
        room.Archive(archivedAt);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomArchivedEvent = domainEvents.OfType<RoomArchivedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomArchivedEvent);
        Assert.Equal(room.Id, roomArchivedEvent.RoomId);
        Assert.Equal(archivedAt, roomArchivedEvent.OccurredAt);
    }

    [Fact]
    public void Room_Delete_ShouldRaiseRoomDeletedDomainEvent_AfterIncactivity()
    {
        // Arrange
        var name = Name.Create("DeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        room.Archive(DateTime.UtcNow.AddHours(1));
        var deletedAt = DateTime.UtcNow.AddHours(2);
        var reason = DeletionReason.Inactivity;
        // Act
        room.Delete(deletedAt, reason);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomDeletedEvent = domainEvents.OfType<RoomDeletedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomDeletedEvent);
        Assert.Equal(room.Id, roomDeletedEvent.RoomId);
        Assert.Equal(reason, roomDeletedEvent.DeletionReason);
        Assert.Equal(deletedAt, roomDeletedEvent.OccurredAt);
    }

    [Fact]
    public void Room_Delete_ShouldDelete_WhenDeleteReasonIsManual()
    {
        // Arrange
        var name = Name.Create("ActiveDeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        var deletedAt = DateTime.UtcNow.AddHours(1);
        var reason = DeletionReason.Manual;
        // Act
        room.Delete(deletedAt, reason);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomDeletedEvent = domainEvents.OfType<RoomDeletedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomDeletedEvent);
        Assert.Equal(room.Id, roomDeletedEvent.RoomId);
        Assert.Equal(reason, roomDeletedEvent.DeletionReason);
        Assert.Equal(deletedAt, roomDeletedEvent.OccurredAt);
    }
    [Fact]
    public void Room_Delete_ShouldThrowError_WhenActiveRoomDeletedDueToInactivity()
    {
        // Arrange
        var name = Name.Create("ActiveDeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        var deletedAt = DateTime.UtcNow.AddHours(1);
        var reason = DeletionReason.Inactivity;
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Delete(deletedAt, reason));
    }

    [Fact]
    public void Room_Archive_ShouldThrowError_WhenRoomIsNotActive()
    {
        // Arrange
        var name = Name.Create("ArchiveTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        var archivedAt = DateTime.UtcNow.AddHours(1);
        room.Archive(archivedAt);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Archive(DateTime.UtcNow.AddHours(2)));
    }

    [Fact]
    public void Room_Delete_ShouldThrowError_WhenRoomIsAlreadyDeleted()
    {
        // Arrange
        var name = Name.Create("DeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(name, capacity, createdAt);
        var deletedAt = DateTime.UtcNow.AddHours(1);
        var reason = DeletionReason.Manual;
        room.Delete(deletedAt, reason);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Delete(DateTime.UtcNow.AddHours(2), reason));
    }

    [Fact]
    public void Room_Rename_ShouldNotRaiseEvent_WhenNameIsSame()
    {
        // Arrange
        var initialName = Name.Create("InitialRoomName");
        var capacity = Capacity.Create(100);
        var createdAt = DateTime.UtcNow;
        var room = Room.Create(initialName, capacity, createdAt);
        // Act
        room.Rename(initialName);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomRenamedEvent = domainEvents.OfType<RoomRenamedDomainEvent>().FirstOrDefault();
        Assert.Null(roomRenamedEvent);
    }

    [Fact]
    public void Room_ChangeCapacity_ShouldNotRaiseEvent_WhenCapacityIsSame()
    {
        // Arrange
        var roomName = Name.Create("CapacityChangeRoom");
        var capacityValue = 75;
        var initialCapacity = Capacity.Create(capacityValue);
        var room = Room.Create(roomName, initialCapacity, DateTime.UtcNow);
        // Act
        room.ChangeCapacity(initialCapacity);
        // Assert
        var domainEvents = room.DomainEvents;
        var capacityChangedEvent = domainEvents.OfType<RoomCapacityChangedDomainEvent>().FirstOrDefault();
        Assert.Null(capacityChangedEvent);
    }

    [Fact]
    public void Name_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var nameValue = "TestRoom";
        var name1 = Name.Create(nameValue);
        var name2 = Name.Create(nameValue);
        // Act & Assert
        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
        Assert.False(name1 != name2);
    }

    [Fact]
    public void Capacity_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var capacityValue = 100;
        var capacity1 = Capacity.Create(capacityValue);
        var capacity2 = Capacity.Create(capacityValue);
        // Act & Assert
        Assert.Equal(capacity1, capacity2);
        Assert.True(capacity1 == capacity2);
        Assert.False(capacity1 != capacity2);
    }

    [Fact]
    public void RoomCode_New_ShouldGenerateValidCode()
    {
        // Act
        var roomCode = RoomCode.New();
        // Assert
        Assert.NotNull(roomCode.Value);
        Assert.Equal(8, roomCode.Value.Length);
    }
}
