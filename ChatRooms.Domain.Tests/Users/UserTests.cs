using ChatRooms.Domain.Tests.Mocks;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidName_ShouldSucceed()
    {
        // Arrange
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        // Act
        var user = User.Create(name, gender, birthDate);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void CreateUser_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(Name.From(string.Empty), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10))));
    }

    [Fact]
    public void CreateUser_ShouldRaiseUserCreatedDomainEvent()
    {
        // Arrange
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        // Act
        var user = User.Create(name, gender, birthDate);
        // Assert
        Assert.Contains(user.DomainEvents, e => e is UserCreatedDomainEvent);
    }

    [Fact]
    public void CreateUser_Should_CreateUserWith_A_NoneDefaultId()
    {
        // Arrange
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        // Act
        var user = User.Create(name, gender, birthDate);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
        Assert.NotEqual(default, user.Id);
    }

    [Fact]
    public void RenameUser_WithValidNewName_ShouldSucceed()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
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
        var user = User.Create(Name.From("SameName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
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
        var user = User.Create(Name.From("ValidName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.Rename(Name.From(string.Empty)));
    }

    [Fact]
    public void RenameUser_ShouldRaiseUserRenamedDomainEvent()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
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
        var user = User.Create(Name.From("InitialName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
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
        var user = User.Create(Name.From("ValidName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.Apply(new UnsupportedDomainEvent()));
    }

    [Fact]
    public void Apply_UserCreatedDomainEvent_ShouldSetProperties()
    {
        // Arrange
        var userId = UserId.New();
        var name = Name.From("TestUser");
        var gender = Gender.Male;
        var birthDate = BirthDate.From(new DateTime(2020, 10, 10));
        var user = User.Create(name, gender, birthDate);
        var domainEvent = new UserCreatedDomainEvent(userId, name, gender, birthDate);
        // Act
        user.Apply(domainEvent);
        // Assert
        Assert.Equal(userId, user.Id);
        Assert.Equal(name, user.Name);
        Assert.Equal(gender, user.Gender);
        Assert.Equal(birthDate, user.BirthDate);
    }

    [Fact]
    public void Apply_UserRenamedDomainEvent_ShouldUpdateName()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"), Gender.Male, BirthDate.From(new DateTime(2020, 10, 10)));
        var newName = Name.From("UpdatedName");
        var domainEvent = new UserRenamedDomainEvent(user.Id, newName);
        // Act
        user.Apply(domainEvent);
        // Assert
        Assert.Equal(newName, user.Name);
    }

    [Theory]
    [InlineData("2020-10-10", 5)]
    [InlineData("2000-01-01", 26)]
    public void AgeCalculation_Should_ReturnCorrectAge(string birthDateString, int expectedYears)
    {
        // Arrange
        var birthDate = BirthDate.From(DateTime.Parse(birthDateString));
        var user = User.Create(Name.From("Test"), Gender.Male, birthDate);
        // Act
        var age = user.Age;
        // Assert
        Assert.Equal(expectedYears, age.Years);
    }

}
