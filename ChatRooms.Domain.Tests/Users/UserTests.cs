using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Tests.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_ShouldCreateUser()
    {
        // Arrange
        var name = Name.From("JohnDoe");
        var occurredAt = DateTimeUtc.NowUtc();
        // Act
        var user = User.Create(name, occurredAt);
        // Assert
        Assert.Equal(name, user.Name);
        Assert.IsType<UserCreatedDomainEvent>(user.DomainEvents.FirstOrDefault());
    }
}
