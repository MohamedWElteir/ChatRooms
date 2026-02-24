using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Tests.Mocks;
using System.Collections.Concurrent;
using ChatRooms.Domain.Tests.Helpers;
namespace ChatRooms.Domain.Tests.Shared;

public sealed class SharedTests
{
    [Fact]
    public void Creating_NewEntity_Should_Not_Set_Id()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
    }


    [Fact]
    public void TestDomainEvent_Should_Store_Provided_OccurredAt()
    {
        // Arrange
        var expected = DateTimeUtc.FromUtc(new DateTime(2024, 1, 1, 12, 0, 0,DateTimeKind.Utc));
        // Act
        var domainEvent = new TestDomainEvent(expected);
        // Assert
        Assert.Equal(expected, domainEvent.OccurredAt);
    }

    [Fact]
    public void DomainEvent_Should_Be_Immutable()
    {
        var domainEvent = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.IsType<TestDomainEvent>(domainEvent);
        Assert.True(
            domainEvent.GetType().GetProperties().All(p =>
                p.CanRead && (!p.CanWrite || TestHelpers.IsInitOnly(p))),
            $"Mutable properties: {string.Join(", ", domainEvent.GetType().GetProperties()
                .Where(p => p.CanWrite && !TestHelpers.IsInitOnly(p))
                .Select(p => p.Name))}"
        );
    }

    

    [Fact]
    public void DomainEvent_Should_Inherit_From_DomainEvent_Base_Class()
    {
        // Arrange
        var domainEvent = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.IsType<DomainEvent>(domainEvent, exactMatch: false);
    }

    [Fact]
    public void DomainEvent_Should_Be_Serializable()
    {
        // Arrange
        var domainEvent = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act
        var serialized = System.Text.Json.JsonSerializer.Serialize(domainEvent);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<TestDomainEvent>(serialized);
        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(domainEvent.OccurredAt.Value,
                    deserialized.OccurredAt.Value);

    }


    [Fact]
    public void DomainEvent_Should_Have_Proper_ToString_Implementation()
    {
        // Arrange
        var domainEvent = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act
        var toStringResult = domainEvent.ToString();
        // Assert
        Assert.False(string.IsNullOrWhiteSpace(toStringResult), "ToString should return a non-empty string representation of the domain event.");
    }

    [Fact]
    public void DomainEvent_Should_Be_Comparable()
    {
        // Arrange
        var domainEvent1 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        var domainEvent2 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.NotEqual(domainEvent1, domainEvent2);
        Assert.Equal(domainEvent1, domainEvent1);
    }

    [Fact]
    public void DomainEvent_Should_Have_OccurredAt_Set_To_Utc()
    {
        // Arrange
        var domainEvent = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.Equal(DateTimeKind.Utc, domainEvent.Kind);
    }

    [Fact]
    public void DomainEvent_Should_Have_Unique_Id()
    {
        // Arrange
        var domainEvent1 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        var domainEvent2 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.NotEqual(domainEvent1.Id, domainEvent2.Id);
    }

    [Fact]
    public void OccurredAt_Should_Be_ThreadSafe()
    {
        var domainEvent = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));

        var results = new ConcurrentBag<DateTime>();

        Parallel.For(0, 10_000, _ =>
        {
            results.Add(domainEvent.OccurredAt.DateTime);
        });

        var distinctValues = results.Distinct().ToList();

        Assert.Single(distinctValues);
        Assert.Equal(DateTimeKind.Utc, distinctValues.First().Kind);
    }


    [Fact]
    public void DomainEvent_Should_Be_Equatable()
    {
        // Arrange
        var domainEvent1 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        var domainEvent2 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.False(domainEvent1.Equals(domainEvent2), "Two different instances of a domain event should not be considered equal.");
        Assert.True(domainEvent1.Equals(domainEvent1), "An instance of a domain event should be considered equal to itself.");
    }

    [Fact]
    public void DomainEvent_Should_Have_Proper_HashCode_Implementation()
    {
        // Arrange
        var domainEvent1 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        var domainEvent2 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act
        var hashCode1 = domainEvent1.GetHashCode();
        var hashCode2 = domainEvent2.GetHashCode();
        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void DomainEvent_Should_Be_Usable_As_Dictionary_Key()
    {
        // Arrange
        var domainEvent1 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        var domainEvent2 = new TestDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow));
        var dictionary = new Dictionary<TestDomainEvent, string>
        {
            // Act
            [domainEvent1] = "First Event",
            [domainEvent2] = "Second Event"
        };
        // Assert
        Assert.Equal("First Event", dictionary[domainEvent1]);
        Assert.Equal("Second Event", dictionary[domainEvent2]);
    }

    [Fact]
    public void DateTimeUtc_ShouldThrowException_ForNonUtcDateTime()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DateTimeUtc.FromUtc(localDateTime));
    }

    [Fact]
    public void DateTimeUtc_ImplicitConversionToDateTime_ShouldWorkCorrectly()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;
        var dateTimeUtc = DateTimeUtc.FromUtc(utcDateTime);
        // Act
        DateTime convertedDateTime = dateTimeUtc.DateTime;
        // Assert
        Assert.Equal(utcDateTime, convertedDateTime);
    }

    [Fact]
    public void DateTimeUtc_AddHours_ShouldReturnNewInstance()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;
        var dateTimeUtc = DateTimeUtc.FromUtc(utcDateTime);
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
        var dateTimeUtc = DateTimeUtc.FromUtc(utcDateTime);
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

    [Fact]
    public void DateTimeUtc_Should_Be_Immutable()
    {
        // Arrange
        var utcDateTime = DateTime.UtcNow;
        var dateTimeUtc = DateTimeUtc.FromUtc(utcDateTime);
        // Act
        var newDateTimeUtc = dateTimeUtc.AddHours(1);
        // Assert
        Assert.NotEqual(dateTimeUtc, newDateTimeUtc);
        Assert.Equal(utcDateTime, dateTimeUtc.Value);
    }

    [Fact]
    public void DateTimeUtc_Should_Have_Proper_Equality()
    {
        // Arrange
        var utcDateTime1 = DateTime.UtcNow;
        var utcDateTime2 = utcDateTime1.AddSeconds(1);
        var dateTimeUtc1 = DateTimeUtc.FromUtc(utcDateTime1);
        var dateTimeUtc2 = DateTimeUtc.FromUtc(utcDateTime2);
        // Act & Assert
        Assert.False(dateTimeUtc1.Equals(dateTimeUtc2), "Two different DateTimeUtc instances should not be considered equal.");
        Assert.True(dateTimeUtc1.Equals(dateTimeUtc1), "An instance of DateTimeUtc should be considered equal to itself.");
    }

    [Fact]
    public void DateTimeUtc_Should_Have_Proper_HashCode_Implementation()
    {
        // Arrange
        var utcDateTime1 = DateTime.UtcNow;
        var utcDateTime2 = utcDateTime1.AddSeconds(1);
        var dateTimeUtc1 = DateTimeUtc.FromUtc(utcDateTime1);
        var dateTimeUtc2 = DateTimeUtc.FromUtc(utcDateTime2);
        // Act
        var hashCode1 = dateTimeUtc1.GetHashCode();
        var hashCode2 = dateTimeUtc2.GetHashCode();
        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }
}
