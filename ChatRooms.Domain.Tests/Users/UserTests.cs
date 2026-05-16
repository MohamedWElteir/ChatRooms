using ChatRooms.Domain.Shared;
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
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);
        // Act
        var user = User.Create(name, email, gender, birthDate, occurredAtUtc);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void CreateUser_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(Name.From(string.Empty),
                                                           Email.From("test@test.com"),
                                                           Gender.Male,
                                                           BirthDate.From(new DateTime(2020, 10, 10)),
                                                           DateTimeUtc.FromUtc(DateTime.UtcNow)));
    }

    [Fact]
    public void CreateUser_ShouldRaiseUserCreatedDomainEvent()
    {
        // Arrange
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);
        // Act
        var user = User.Create(name, email, gender, birthDate, occurredAtUtc);
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
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);
        // Act
        var user = User.Create(name, email, gender, birthDate, occurredAtUtc);
        // Assert
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
        Assert.NotEqual(default, user.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Null_Email_Should_Throw(string? nullOrWhiteSpaceString)
    {
        Assert.Throws<ArgumentNullException>(() => Email.From(nullOrWhiteSpaceString!));
    }

    [Theory]
    [InlineData("invalid.com")]        // missing @
    [InlineData("@nodomain.com")]      // missing local part
    [InlineData("no@")]                // missing domain
    [InlineData("no@domain")]          // missing TLD
    [InlineData("spaces in@email.com")]// spaces in local part
    [InlineData("double@@email.com")]  // double @
    [InlineData("missing.dot@com")]    // no dot in domain
    [InlineData("@.com")]              // missing domain name
    [InlineData("user@.com")]          // domain starts with dot
    [InlineData("user@domain..com")]   // consecutive dots in domain
    [InlineData("user@domain.com.")]   // dot in the end
    [InlineData("user$@domain.com")]   // invalid character
    public void Create_Invalid_Email_Should_Throw(string invalidEmail)
    {
        Assert.Throws<ArgumentException>(() => Email.From(invalidEmail));
    }

    [Fact]
    public void RenameUser_WithValidNewName_ShouldSucceed()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"),
                                         Email.From("test@test.com"),
                                         Gender.Male,
                                         BirthDate.From(new DateTime(2020, 10, 10)),
                                         DateTimeUtc.FromUtc(DateTime.UtcNow));
        var newName = Name.From("NewValidName");
        // Act
        user.Rename(newName, DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Assert
        Assert.Equal(newName, user.Name);
    }

    [Fact]
    public void RenameUser_WithSameName_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = User.Create(Name.From("SameName"),
                                    Email.From("test@test.com"),
                                    Gender.Male,
                                    BirthDate.From(new DateTime(2020, 10, 10)),
                                    DateTimeUtc.FromUtc(DateTime.UtcNow));
        var sameName = Name.From("SameName");
        // Act
        user.Rename(sameName, DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Assert
        Assert.DoesNotContain(user.DomainEvents, e => e is UserRenamedDomainEvent);

    }
    [Fact]
    public void RenameUser_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var user = User.Create(Name.From("ValidName"),
                                    Email.From("test@test.com"),
                                    Gender.Male,
                                    BirthDate.From(new DateTime(2020, 10, 10)),
                                    DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.Rename(Name.From(string.Empty), DateTimeUtc.FromUtc(DateTime.UtcNow)));
    }

    [Fact]
    public void RenameUser_ShouldRaiseUserRenamedDomainEvent()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"),
                                    Email.From("test@test.com"),
                                    Gender.Male,
                                    BirthDate.From(new DateTime(2020, 10, 10)),
                                    DateTimeUtc.FromUtc(DateTime.UtcNow));
        var newName = Name.From("NewValidName");
        // Act
        user.Rename(newName, DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Assert
        Assert.Contains(user.DomainEvents, e => e is UserRenamedDomainEvent);
    }

    [Fact]
    public void RenameUser_Should_UpdateNameProperty()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"),
                                    Email.From("test@test.com"),
                                    Gender.Male,
                                    BirthDate.From(new DateTime(2020, 10, 10)),
                                    DateTimeUtc.FromUtc(DateTime.UtcNow));
        var newName = Name.From("NewValidName");
        // Act
        user.Rename(newName, DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Assert
        Assert.Equal(newName, user.Name);
    }

    [Fact]
    public void Apply_UnsupportedEvent_ShouldThrowException()
    {
        // Arrange
        var user = User.Create(Name.From("ValidName"),
                                    Email.From("test@test.com"),
                                    Gender.Male,
                                    BirthDate.From(new DateTime(2020, 10, 10)),
                                    DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => user.Apply(new UnsupportedDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow))));
    }

    [Fact]
    public void Apply_UserCreatedDomainEvent_ShouldSetProperties()
    {
        // Arrange
        var userId = UserId.New();
        var name = Name.From("TestUser");
        var gender = Gender.Male;
        var birthDate = BirthDate.From(new DateTime(2020, 10, 10));
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);
        var user = User.Create(name, email, gender, birthDate, occurredAtUtc);
        var domainEvent = new UserCreatedDomainEvent(userId, name, email, gender, birthDate, occurredAtUtc);
        // Act
        user.Apply(domainEvent);
        // Assert
        Assert.Equal(userId, user.Id);
        Assert.Equal(name, user.Name);
        Assert.Equal(gender, user.Gender);
        Assert.Equal(email, user.Email);
        Assert.Equal(birthDate, user.BirthDate);
        Assert.Equal(occurredAtUtc, user.CreatedAt);
    }

    [Fact]
    public void Apply_UserRenamedDomainEvent_ShouldUpdateName()
    {
        // Arrange
        var user = User.Create(Name.From("InitialName"),
                                        Email.From("test@test.com"),
                                        Gender.Male,
                                        BirthDate.From(new DateTime(2020, 10, 10)),
                                        DateTimeUtc.FromUtc(DateTime.UtcNow));
        var newName = Name.From("UpdatedName");
        var domainEvent = new UserRenamedDomainEvent(user.Id, newName, DateTimeUtc.FromUtc(DateTime.UtcNow));
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
        var user = User.Create(Name.From("Test"),
                                        Email.From("test@test.com"),
                                        Gender.Male,
                                        birthDate,
                                        DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Act
        var age = user.Age;
        // Assert
        Assert.Equal(expectedYears, age.Years);
    }

    [Fact]
    public void Change_Email_Should_Return_New_Email()
    {
        // Arrange
        var user = User.Create(Name.From("Test"),
                                Email.From("test@test.com"),
                                Gender.Male,
                                BirthDate.From(new DateTime(2020, 10, 10)),
                                DateTimeUtc.FromUtc(DateTime.UtcNow));
        var newEmail = Email.From("new@test.com");
        // Act
        user.ChangeEmail(newEmail, DateTimeUtc.FromUtc(DateTime.UtcNow));
        // Assert
        Assert.Equal(newEmail, user.Email);
    }

}