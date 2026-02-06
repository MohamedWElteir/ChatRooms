using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Tests.Rooms.Mocks;

namespace ChatRooms.Domain.Tests.Rooms;

public sealed class RoomTests
{
    [Fact]
    public void CreateRoom_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var name = Name.From("GeneralChat");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
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
        var name = Name.From("TechTalk");
        var capacity = Capacity.Create(50);
        var createdAt = DateTimeUtc.NowUtc();
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
        var createdAt = DateTimeUtc.NowUtc();
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
        var name = Name.From("EventClearTest");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
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
        var capacityValue = 75;
        var initialCapacity = Capacity.Create(capacityValue);
        var room = Room.Create(roomName, initialCapacity, DateTimeUtc.NowUtc());
        var newCapacityValue = 100;
        var newCapacity = Capacity.Create(newCapacityValue);
        // Act
        room.ChangeCapacity(newCapacity, DateTimeUtc.NowUtc());
        // Assert
        Assert.Equal(newCapacityValue, room.Capacity.Value);
    }

    [Fact]
    public void Room_Rename_ShouldRaiseRoomRenamedDomainEvent()
    {
        // Arrange
        var initialName = Name.From("InitialRoomName");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(initialName, capacity, createdAt);
        var newName = Name.From("RenamedRoom");
        // Act
        room.Rename(newName,DateTimeUtc.NowUtc());
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
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        var archivedAt = DateTimeUtc.NowUtc().AddHours(1);
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
    public void Room_Delete_ShouldRaiseRoomDeletedDomainEvent_AfterInactivity()
    {
        // Arrange
        var name = Name.From("DeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        room.Archive(DateTimeUtc.NowUtc().AddHours(1));
        var deletedAt = DateTimeUtc.NowUtc().AddHours(2);
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
        var name = Name.From("ActiveDeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        var deletedAt = DateTimeUtc.NowUtc().AddHours(1);
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
        var name = Name.From("ActiveDeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        var deletedAt = DateTimeUtc.NowUtc().AddHours(1);
        var reason = DeletionReason.Inactivity;
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Delete(deletedAt, reason));
    }

    [Fact]
    public void Room_Archive_ShouldThrowError_WhenRoomIsNotActive()
    {
        // Arrange
        var name = Name.From("ArchiveTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        var archivedAt = DateTimeUtc.NowUtc().AddHours(1);
        room.Archive(archivedAt);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Archive(DateTimeUtc.NowUtc().AddHours(2)));
    }

    [Fact]
    public void Room_Delete_ShouldThrowError_WhenRoomIsAlreadyDeleted()
    {
        // Arrange
        var name = Name.From("DeleteTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        var deletedAt = DateTimeUtc.NowUtc().AddHours(1);
        var reason = DeletionReason.Manual;
        room.Delete(deletedAt, reason);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Delete(DateTimeUtc.NowUtc().AddHours(2), reason));
    }

    [Fact]
    public void Room_Rename_ShouldNotRaiseEvent_WhenNameIsSame()
    {
        // Arrange
        var initialName = Name.From("InitialRoomName");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(initialName, capacity, createdAt);
        // Act
        room.Rename(initialName, DateTimeUtc.NowUtc());
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
        var room = Room.Create(roomName, initialCapacity, DateTimeUtc.NowUtc());
        // Act
        room.ChangeCapacity(initialCapacity, DateTimeUtc.NowUtc());
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
    public void Room_Code_ToString_ShouldReturnCodeValue()
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
        var room = Room.Create(Name.From("TestRoom"), Capacity.Create(100), DateTimeUtc.NowUtc());
        var unsupportedEvent = new UnsupportedDomainEvent(DateTimeUtc.NowUtc());
        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => room.Apply(unsupportedEvent));
        Assert.Equal($"Event '{unsupportedEvent.GetType().Name}' is not supported by {nameof(Room)}", exception?.Result.Message);
    }

    [Fact]
    public void DateTimeUtc_ShouldThrowException_ForNonUtcDateTime()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DateTimeUtc(localDateTime));
    }

    [Fact]
    public void DateTimeUtc_ImplicitConversionToDateTime_ShouldWorkCorrectly()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;
        var dateTimeUtc = new DateTimeUtc(utcDateTime);
        // Act
        DateTime convertedDateTime = dateTimeUtc;
        // Assert
        Assert.Equal(utcDateTime, convertedDateTime);
    }

    [Fact]
    public void DateTimeUtc_AddHours_ShouldReturnNewInstance()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;
        var dateTimeUtc = new DateTimeUtc(utcDateTime);
        var hoursToAdd = 5;
        // Act
        var newDateTimeUtc = dateTimeUtc.AddHours(hoursToAdd);
        // Assert
        Assert.Equal(utcDateTime.AddHours(hoursToAdd), newDateTimeUtc.Value);
        Assert.NotEqual(dateTimeUtc, newDateTimeUtc);
    }

    [Fact]
    public void DateTimeUtc_ToString_ShouldReturnIso8601Format()
    {
        // Arrange
        var utcDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var dateTimeUtc = new DateTimeUtc(utcDateTime);
        // Act
        var dateTimeString = dateTimeUtc.ToString();
        // Assert
        Assert.Equal(utcDateTime.ToString("o"), dateTimeString);
    }

    [Fact]
    public void DateTimeUtc_FromLocal_ShouldConvertToUtc()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
        // Act
        var dateTimeUtc = DateTimeUtc.FromLocal(localDateTime);
        // Assert
        Assert.Equal(localDateTime.ToUniversalTime(), dateTimeUtc.Value);
    }

    [Fact]
    public void DateTimeUtc_NowUtc_ShouldReturnCurrentUtcTime()
    {
        // Act
        var dateTimeUtc = DateTimeUtc.NowUtc();
        // Assert
        var nowUtc = DateTime.UtcNow;
        Assert.InRange(dateTimeUtc.Value, nowUtc.AddSeconds(-1), nowUtc.AddSeconds(1));
    }
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void Room_ChangeCapacity_ShouldThrowException_WhenNewCapacityLessThanCurrentParticipants(int currentParticipants)
    {
        // Arrange
        var roomName = Name.From("CapacityTestRoom");
        var initialCapacity = Capacity.Create(100);
        var room = Room.Create(roomName, initialCapacity, DateTimeUtc.NowUtc());
        for (int i = 0; i < currentParticipants; i++)
        {
            room.Join(DateTimeUtc.NowUtc().AddMinutes(i + 1));
        }
        var newCapacity = Capacity.Create(currentParticipants - 1);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.ChangeCapacity(newCapacity, DateTimeUtc.NowUtc()));
    }

    [Fact]
    public void RoomParticipantJoin_And_Leave_ShouldUpdateParticipantCountCorrectly()
    {
        // Arrange
        var roomName = Name.From("ParticipantCountRoom");
        var capacity = Capacity.Create(2);
        var room = Room.Create(roomName, capacity, DateTimeUtc.NowUtc());
        var joinTime = DateTimeUtc.NowUtc().AddMinutes(1);
        var leaveTime = DateTimeUtc.NowUtc().AddMinutes(2);
        // Act
        room.Join(joinTime);
        Assert.Equal(1, room.CurrentParticipantsCount);
        room.Join(DateTimeUtc.NowUtc().AddMinutes(3));
        Assert.Equal(2, room.CurrentParticipantsCount);
        room.Leave(leaveTime);
        // Assert
        Assert.Equal(1, room.CurrentParticipantsCount);
    }
    [Fact]
    public void RoomParticipantLeave_ShouldThrowException_WhenNoParticipants()
    {
        // Arrange
        var roomName = Name.From("LeaveNoParticipantsRoom");
        var capacity = Capacity.Create(10);
        var room = Room.Create(roomName, capacity, DateTimeUtc.NowUtc());
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Leave(DateTimeUtc.NowUtc().AddMinutes(1)));
    }
    [Fact]
    public void RoomParticipantJoin_ShouldThrowException_WhenCapacityReached()
    {
        // Arrange
        var roomName = Name.From("CapacityReachedRoom");
        var capacity = Capacity.Create(2);
        var room = Room.Create(roomName, capacity, DateTimeUtc.NowUtc());
        room.Join(DateTimeUtc.NowUtc().AddMinutes(1));
        room.Join(DateTimeUtc.NowUtc().AddMinutes(2));
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Join(DateTimeUtc.NowUtc().AddMinutes(3)));
    }

    [Fact]
    public void Room_Restore_ShouldRaiseRoomRestoredDomainEvent()
    {
        // Arrange
        var name = Name.From("RestoreTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        room.Archive(DateTimeUtc.NowUtc().AddHours(1));
        var restoredAt = DateTimeUtc.NowUtc().AddHours(2);
        // Act
        room.Restore(restoredAt);
        // Assert
        var domainEvents = room.DomainEvents;
        var roomRestoredEvent = domainEvents.OfType<RoomRestoredDomainEvent>().FirstOrDefault();
        Assert.NotNull(roomRestoredEvent);
        Assert.Equal(restoredAt, roomRestoredEvent.OccurredAt);
    }

    [Fact]
    public void Room_Restore_ShouldThrowError_WhenRoomIsNotArchived()
    {
        // Arrange
        var name = Name.From("RestoreTestRoom");
        var capacity = Capacity.Create(100);
        var createdAt = DateTimeUtc.NowUtc();
        var room = Room.Create(name, capacity, createdAt);
        var restoredAt = DateTimeUtc.NowUtc().AddHours(1);
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.Restore(restoredAt));
    }
}
