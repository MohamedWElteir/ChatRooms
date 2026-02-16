using ChatRooms.Domain.Tests.Mocks;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Tests.Aggregates;

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

    [Fact]
    public void CreateUser_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(Name.From(string.Empty)));
    }

    [Fact]
    public void CreateUser_ShouldRaiseUserCreatedDomainEvent()
    {
        // Arrange
        var name = Name.From("ValidUserName");
        // Act
        var user = User.Create(name);
        // Assert
        Assert.Contains(user.DomainEvents, e => e is UserCreatedDomainEvent);
    }

    [Fact]
    public void CreateUser_Should_CreateUserWith_A_NoneDefaultId()
    {
        // Arrange
        var name = Name.From("ValidUserName");
        var user = User.Create(name);
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
        Assert.NotEqual(default, user.Id);
    }

    [Fact]
    public void RenameUser_WithValidNewName_ShouldSucceed()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"));
        var newName = Name.From("NewValidName");
        // Act
        user.Rename(newName);
        // Assert
        Assert.Equal(newName, user.Name);
    }

    [Fact]
    public void RenameUser_WithSameName_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = User.Create(Name.From("SameName"));
        var sameName = Name.From("SameName");
        // Act
        user.Rename(sameName);
        // Assert
        Assert.DoesNotContain(user.DomainEvents, e => e is UserRenamedDomainEvent);

    }
    [Fact]
    public void RenameUser_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var user = User.Create(Name.From("ValidName"));
        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.Rename(Name.From(string.Empty)));
    }

    [Fact]
    public void RenameUser_ShouldRaiseUserRenamedDomainEvent()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"));
        var newName = Name.From("NewValidName");
        // Act
        user.Rename(newName);
        // Assert
        Assert.Contains(user.DomainEvents, e => e is UserRenamedDomainEvent);
    }

    [Fact]
    public void RenameUser_Should_UpdateNameProperty()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"));
        var newName = Name.From("NewValidName");
        // Act
        user.Rename(newName);
        // Assert
        Assert.Equal(newName, user.Name);
    }

    [Fact]
    public void Apply_UnsupportedEvent_ShouldThrowException()
    {
        // Arrange
        var user = User.Create(Name.From("ValidName"));
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.Apply(new UnsupportedDomainEvent()));
    }

    [Fact]
    public void Apply_UserCreatedDomainEvent_ShouldSetProperties()
    {
        // Arrange
        var userId = UserId.New();
        var name = Name.From("TestUser");
        var user = User.Create(name);
        var domainEvent = new UserCreatedDomainEvent(userId, name);
        // Act
        user.Apply(domainEvent);
        // Assert
        Assert.Equal(userId, user.Id);
        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void Apply_UserRenamedDomainEvent_ShouldUpdateName()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"));
        var newName = Name.From("UpdatedName");
        var domainEvent = new UserRenamedDomainEvent(user.Id, newName);
        // Act
        user.Apply(domainEvent);
        // Assert
        Assert.Equal(newName, user.Name);
    }

}
