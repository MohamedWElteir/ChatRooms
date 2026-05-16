using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Shared.Errors;
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
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);

        var userResult = User.Create(name, email, gender, birthDate, occurredAtUtc);
        var user = userResult.Value!;

        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void CreateUser_WithEmptyName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => User.Create(Name.From(string.Empty),
                                                           Email.From("test@test.com"),
                                                           Gender.Male,
                                                           BirthDate.From(new DateTime(2020, 10, 10)),
                                                           DateTimeUtc.FromUtc(DateTime.UtcNow)));
    }

    [Fact]
    public void CreateUser_ShouldRaiseUserCreatedDomainEvent()
    {
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);

        var userResult = User.Create(name, email, gender, birthDate, occurredAtUtc);
        var user = userResult.Value!;

        Assert.Contains(user.DomainEvents, e => e is UserCreatedDomainEvent);
    }

    [Fact]
    public void CreateUser_Should_CreateUserWith_A_NoneDefaultId()
    {
        var name = Name.From("ValidUserName");
        var birthDate = BirthDate.From(new DateTime(2025, 10, 10));
        var gender = Gender.Male;
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);

        var userResult = User.Create(name, email, gender, birthDate, occurredAtUtc);
        var user = userResult.Value!;

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
    [InlineData("invalid.com")]
    [InlineData("@nodomain.com")]
    [InlineData("no@")]
    [InlineData("no@domain")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@email.com")]
    [InlineData("missing.dot@com")]
    [InlineData("@.com")]
    [InlineData("user@.com")]
    [InlineData("user@domain..com")]
    [InlineData("user@domain.com.")]
    [InlineData("user$@domain.com")]
    public void Create_Invalid_Email_Should_Throw(string invalidEmail)
    {
        Assert.Throws<ArgumentException>(() => Email.From(invalidEmail));
    }

    [Fact]
    public void RenameUser_WithValidNewName_ShouldSucceed()
    {
        var userResult = User.Create(Name.From("InitialName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        var newName = Name.From("NewValidName");

        user.Rename(newName, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Equal(newName, user.Name);
    }

    [Fact]
    public void RenameUser_WithSameName_ShouldNotRaiseEvent()
    {
        var userResult = User.Create(Name.From("SameName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        var sameName = Name.From("SameName");

        user.Rename(sameName, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.DoesNotContain(user.DomainEvents, e => e is UserRenamedDomainEvent);
    }

    [Fact]
    public void RenameUser_WithEmptyName_ShouldThrowException()
    {
        var userResult = User.Create(Name.From("ValidName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        Assert.Throws<ArgumentException>(() => user.Rename(Name.From(string.Empty), DateTimeUtc.FromUtc(DateTime.UtcNow)));
    }

    [Fact]
    public void RenameUser_ShouldRaiseUserRenamedDomainEvent()
    {
        var userResult = User.Create(Name.From("InitialName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        var newName = Name.From("NewValidName");

        user.Rename(newName, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Contains(user.DomainEvents, e => e is UserRenamedDomainEvent);
    }

    [Fact]
    public void RenameUser_Should_UpdateNameProperty()
    {
        var userResult = User.Create(Name.From("InitialName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        var newName = Name.From("NewValidName");

        user.Rename(newName, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Equal(newName, user.Name);
    }

    [Fact]
    public void RenameUser_WhenDeleted_ShouldFail()
    {
        var userResult = User.Create(Name.From("InitialName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var renameResult = user.Rename(Name.From("NewName"), DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.True(renameResult.IsFailure);
    }

    [Fact]
    public void DeleteUser_ShouldSucceed()
    {
        var userResult = User.Create(Name.From("ValidName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        var deleteResult = user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.True(deleteResult.IsSuccess);
        Assert.True(user.IsDeleted);
    }

    [Fact]
    public void DeleteUser_WhenAlreadyDeleted_ShouldFail()
    {
        var userResult = User.Create(Name.From("ValidName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var deleteResult = user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.True(deleteResult.IsFailure);
    }

    [Fact]
    public void DeleteUser_ShouldRaiseUserDeletedDomainEvent()
    {
        var userResult = User.Create(Name.From("ValidName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Contains(user.DomainEvents, e => e is UserDeletedDomainEvent);
    }

    [Fact]
    public void Apply_UnsupportedEvent_ShouldThrowException()
    {
        var userResult = User.Create(Name.From("ValidName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        Assert.Throws<InvalidOperationException>(() => user.Apply(new UnsupportedDomainEvent(DateTimeUtc.FromUtc(DateTime.UtcNow))));
    }

    [Fact]
    public void Apply_UserCreatedDomainEvent_ShouldSetProperties()
    {
        var userId = UserId.New();
        var name = Name.From("TestUser");
        var gender = Gender.Male;
        var birthDate = BirthDate.From(new DateTime(2020, 10, 10));
        var email = Email.From("test@test.com");
        var occurredAtUtc = DateTimeUtc.FromUtc(DateTime.UtcNow);
        var userResult = User.Create(name, email, gender, birthDate, occurredAtUtc);
        var user = userResult.Value!;
        var domainEvent = new UserCreatedDomainEvent(userId, name, email, gender, birthDate, occurredAtUtc);

        user.Apply(domainEvent);

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
        var userResult = User.Create(Name.From("InitialName"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        var newName = Name.From("UpdatedName");
        var domainEvent = new UserRenamedDomainEvent(user.Id, newName, DateTimeUtc.FromUtc(DateTime.UtcNow));

        user.Apply(domainEvent);

        Assert.Equal(newName, user.Name);
    }

    [Theory]
    [InlineData("2020-10-10", 5)]
    [InlineData("2000-01-01", 26)]
    public void AgeCalculation_Should_ReturnCorrectAge(string birthDateString, int expectedYears)
    {
        var birthDate = BirthDate.From(DateTime.Parse(birthDateString));
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     birthDate,
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        var age = user.Age;

        Assert.Equal(expectedYears, age.Years);
    }

    [Fact]
    public void Change_Email_Should_Return_New_Email()
    {
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        var newEmail = Email.From("new@test.com");

        user.ChangeEmail(newEmail, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Equal(newEmail, user.Email);
    }

    [Fact]
    public void Change_Email_WithSameEmail_ShouldNotRaiseEvent()
    {
        var email = Email.From("test@test.com");
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        user.ChangeEmail(email, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.DoesNotContain(user.DomainEvents, e => e is UserEmailChangedDomainEvent);
    }

    [Fact]
    public void Change_Email_WhenDeleted_ShouldFail()
    {
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var result = user.ChangeEmail(Email.From("new@test.com"), DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Change_Gender_ShouldSucceed()
    {
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        user.ChangeGender(Gender.Female, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Equal(Gender.Female, user.Gender);
    }

    [Fact]
    public void Change_Gender_WithSameGender_ShouldNotRaiseEvent()
    {
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        user.ChangeGender(Gender.Male, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.DoesNotContain(user.DomainEvents, e => e is UserGenderChangedDomainEvent);
    }

    [Fact]
    public void Change_Gender_WhenDeleted_ShouldFail()
    {
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;
        user.Delete(DeletionReason.DeletedByUser, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var result = user.ChangeGender(Gender.Female, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Change_Gender_ShouldRaiseUserGenderChangedDomainEvent()
    {
        var userResult = User.Create(Name.From("Test"),
                                     Email.From("test@test.com"),
                                     Gender.Male,
                                     BirthDate.From(new DateTime(2020, 10, 10)),
                                     DateTimeUtc.FromUtc(DateTime.UtcNow));
        var user = userResult.Value!;

        user.ChangeGender(Gender.Female, DateTimeUtc.FromUtc(DateTime.UtcNow));

        Assert.Contains(user.DomainEvents, e => e is UserGenderChangedDomainEvent);
    }
}
