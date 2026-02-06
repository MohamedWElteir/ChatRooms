using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidName_ShouldSucceed()
    {
        // Arrange
      var name = Name.From("ValidUserName");
        // Act
        var user = User.Create(name);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
    }
}
