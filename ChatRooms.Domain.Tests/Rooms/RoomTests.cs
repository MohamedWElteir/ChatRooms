using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Tests.Mocks;

namespace ChatRooms.Domain.Tests.Rooms;

public sealed class RoomTests
{
    [Fact]
    public void CreateRoom_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var name = Name.From("GeneralChat");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        // Assert
        Assert.Equal(name, room.Name);
        Assert.Equal(capacity, room.Capacity);
        Assert.NotNull(room.Code.Value);
        Assert.Equal(8, room.Code.Value.Length);
    }

    [Fact]
    public void CreateRoom_Should_CreateRoomWith_A_NoneDefaultId()
    {
        // Arrange
        var name = Name.From("ValidRoomName");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        Assert.NotNull(room);
        Assert.Equal(name, room.Name);
        Assert.NotEqual(default, room.Id);
    }

    [Fact]
    public void Room_Create_ShouldRaiseRoomCreatedDomainEvent()
    {
        // Arrange
        var name = Name.From("TechTalk");
        var capacity = Capacity.Create(50);
        // Act
        var room = Room.Create(name, capacity);
        // Assert
        var domainEvents = room.DomainEvents;
        Assert.Single(domainEvents);
        var roomCreatedEvent = Assert.IsType<RoomCreatedDomainEvent>(domainEvents.First());
        Assert.Equal(room.Id, roomCreatedEvent.RoomId);
        Assert.Equal(name, roomCreatedEvent.Name);
        Assert.Equal(capacity, roomCreatedEvent.Capacity);
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
            Assert.Throws<ArgumentException>(() => Name.From(invalidName));
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
        var name = Name.From("EqualityTest");
        var capacity = Capacity.Create(100);
        var room1 = Room.Create(name, capacity);
        var room2 = Room.Create(name, capacity);
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
        var name = Name.From("EventClearTest");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
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
        var name = Name.From(nameValue);
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
        var roomName = Name.From("CapacityChangeRoom");
        var initialCapacity = Capacity.Create(75);
        var room = Room.Create(roomName, initialCapacity);
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
        var initialName = Name.From("InitialRoomName");
        var capacity = Capacity.Create(100);
        var room = Room.Create(initialName, capacity);
        var newName = Name.From("RenamedRoom");
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
        var name = Name.From("ArchiveTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        // Act
        room.Archive();
        // Assert
        var domainEvents = room.DomainEvents;
        var roomArchivedEvent = domainEvents.OfType<RoomArchivedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomArchivedEvent);
        Assert.Equal(room.Id, roomArchivedEvent.RoomId);
    }

    [Fact]
    public void Room_Delete_ShouldRaiseRoomDeletedDomainEvent_AfterInactivity()
    {
        // Arrange
        var name = Name.From("DeleteTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        room.Archive();
        var reason = DeletionReason.Inactivity;
        // Act
        room.Delete(reason);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomDeletedEvent = domainEvents.OfType<RoomDeletedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomDeletedEvent);
        Assert.Equal(room.Id, roomDeletedEvent.RoomId);
        Assert.Equal(reason, roomDeletedEvent.DeletionReason);
    }

    [Fact]
    public void Room_Delete_ShouldDelete_WhenDeleteReasonIsManual()
    {
        // Arrange
        var name = Name.From("ActiveDeleteTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        var reason = DeletionReason.Manual;
        // Act
        room.Delete(reason);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomDeletedEvent = domainEvents.OfType<RoomDeletedDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomDeletedEvent);
        Assert.Equal(room.Id, roomDeletedEvent.RoomId);
        Assert.Equal(reason, roomDeletedEvent.DeletionReason);
    }
    [Fact]
    public void Room_Delete_ShouldThrowError_WhenActiveRoomDeletedDueToInactivity()
    {
        // Arrange
        var name = Name.From("ActiveDeleteTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        var reason = DeletionReason.Inactivity;
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Delete(reason));
    }

    [Fact]
    public void Room_Archive_ShouldThrowError_WhenRoomIsNotActive()
    {
        // Arrange
        var name = Name.From("ArchiveTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        room.Archive();
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Archive());
    }

    [Fact]
    public void Room_Delete_ShouldThrowError_WhenRoomIsAlreadyDeleted()
    {
        // Arrange
        var name = Name.From("DeleteTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        var reason = DeletionReason.Manual;
        room.Delete(reason);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Delete(reason));
    }

    [Fact]
    public void Room_Rename_ShouldNotRaiseEvent_WhenNameIsSame()
    {
        // Arrange
        var initialName = Name.From("InitialRoomName");
        var capacity = Capacity.Create(100);
        var room = Room.Create(initialName, capacity);
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
        var roomName = Name.From("CapacityChangeRoom");
        var capacityValue = 75;
        var initialCapacity = Capacity.Create(capacityValue);
        var room = Room.Create(roomName, initialCapacity);
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
        var name1 = Name.From(nameValue);
        var name2 = Name.From(nameValue);
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

    [Fact]
    public void RoomCode_ToString_ShouldReturnCodeValue()
    {
        // Arrange
        var roomCode = RoomCode.New();
        // Act
        var codeString = roomCode.ToString();
        // Assert
        Assert.Equal(roomCode.Value, codeString);
    }

    [Fact]
    public void Room_ShouldThrowForUnsupportedEvent()
    {
        // Arrange
        var room = Room.Create(Name.From("TestRoom"), Capacity.Create(100));
        var unsupportedEvent = new UnsupportedDomainEvent();
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => room.Apply(unsupportedEvent));
        Assert.Equal($"Event '{unsupportedEvent.GetType().Name}' is not supported by {nameof(Room)}", exception?.Result.Message);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void Room_ChangeCapacity_ShouldThrowException_WhenNewCapacityLessThanCurrentParticipants(int currentParticipants)
    {
        // Arrange
        var roomName = Name.From("CapacityTestRoom");
        var initialCapacity = Capacity.Create(100);
        var room = Room.Create(roomName, initialCapacity);
        for (int i = 0; i < currentParticipants; i++)
        {
            room.Join();
        }
        var newCapacity = Capacity.Create(currentParticipants - 1);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.ChangeCapacity(newCapacity));
    }

    [Fact]
    public void RoomParticipantJoin_And_Leave_ShouldUpdateParticipantCountCorrectly()
    {
        // Arrange
        var roomName = Name.From("ParticipantCountRoom");
        var capacity = Capacity.Create(2);
        var room = Room.Create(roomName, capacity);
        // Act
        room.Join();
        Assert.Equal(1, room.CurrentParticipantsCount);
        room.Join();
        Assert.Equal(2, room.CurrentParticipantsCount);
        room.Leave();
        // Assert
        Assert.Equal(1, room.CurrentParticipantsCount);
    }
    [Fact]
    public void RoomParticipantLeave_ShouldThrowException_WhenNoParticipants()
    {
        // Arrange
        var roomName = Name.From("LeaveNoParticipantsRoom");
        var capacity = Capacity.Create(10);
        var room = Room.Create(roomName, capacity);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Leave());
    }
    [Fact]
    public void RoomParticipantJoin_ShouldThrowException_WhenCapacityReached()
    {
        // Arrange
        var roomName = Name.From("CapacityReachedRoom");
        var capacity = Capacity.Create(2);
        var room = Room.Create(roomName, capacity);
        room.Join();
        room.Join();
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Join());
    }

    [Fact]
    public void Room_Restore_ShouldRaiseRoomRestoredDomainEvent()
    {
        // Arrange
        var name = Name.From("RestoreTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        room.Archive();
        // Act
        room.Restore();
        // Assert
        var domainEvents = room.DomainEvents;
        var roomRestoredEvent = domainEvents.OfType<RoomRestoredDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomRestoredEvent);
    }

    [Fact]
    public void Room_Restore_ShouldThrowError_WhenRoomIsNotArchived()
    {
        // Arrange
        var name = Name.From("RestoreTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Restore());
    }

    [Fact]
    public void Room_Restore_ShouldChangeStatusToActive()
    {
        // Arrange
        var name = Name.From("RestoreStatusTestRoom");
        var capacity = Capacity.Create(100);
        var room = Room.Create(name, capacity);
        room.Archive();
        // Act
        room.Restore();
        // Assert
        Assert.Equal(RoomStatus.Active, room.Status);
    }
}
