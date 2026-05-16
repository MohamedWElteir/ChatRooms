using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Shared.Enums;
using ChatRooms.Domain.Shared.Errors;
using ChatRooms.Domain.Users.Enums;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    public Name Name { get; private set; }
    public Email Email { get; private set; }
    public Gender Gender { get; private set; }
    public BirthDate BirthDate { get; private set; }
    public Age Age => BirthDate.CalculateAge();

    private User() : base() { }

    public static Result<User> Create(Name name, Email email, Gender gender, BirthDate birthDate, DateTimeUtc OccurredAt)
    {
        var user = new User();
        if (!user.IsTransient())
            return UserErrors.NotTransient;

        user.Raise(new UserCreatedDomainEvent(UserId.New(), name, email, gender, birthDate, OccurredAt));
        return user;
    }

    public override void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedDomainEvent e:
                Apply(e);
                break;
            case UserRenamedDomainEvent e:
                Apply(e);
                break;
            case UserDeletedDomainEvent e:
                Apply(e);
                break;
            case UserEmailChangedDomainEvent e:
                Apply(e);
                break;
            case UserGenderChangedDomainEvent e:
                Apply(e);
                break;
            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(User)}");
        }
    }

    public Result Rename(Name newName, DateTimeUtc occurredAt)
    {
        var check = EnsureNotDeleted();
        if (check.IsFailure) return check;

        if (Name == newName)
            return Result.Success();

        Raise(new UserRenamedDomainEvent(Id, newName, occurredAt));
        return Result.Success();
    }

    public Result Delete(DeletionReason reason, DateTimeUtc occurredAt)
    {
        if (IsDeleted)
            return UserErrors.AlreadyDeleted;

        Raise(new UserDeletedDomainEvent(Id, reason, occurredAt));
        return Result.Success();
    }

    public Result ChangeEmail(Email newEmail, DateTimeUtc occurredAt)
    {
        var check = EnsureNotDeleted();
        if (check.IsFailure) return check;

        if (Email == newEmail)
            return Result.Success();

        Raise(new UserEmailChangedDomainEvent(Id, newEmail, occurredAt));
        return Result.Success();
    }

    public Result ChangeGender(Gender newGender, DateTimeUtc occurredAt)
    {
        var check = EnsureNotDeleted();
        if (check.IsFailure) return check;

        if (Gender == newGender)
            return Result.Success();

        Raise(new UserGenderChangedDomainEvent(Id, newGender, occurredAt));
        return Result.Success();
    }

    #region Event Appliers
    private void Apply(UserCreatedDomainEvent @event)
    {
        Id = @event.UserId;
        Name = @event.Name;
        Gender = @event.Gender;
        Email = @event.Email;
        BirthDate = @event.BirthDate;
        CreatedAt = @event.OccurredAt;
    }

    private void Apply(UserRenamedDomainEvent @event)
    {
        Name = @event.NewName;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(UserDeletedDomainEvent @event)
    {
        DeletedAt = @event.OccurredAt;
        Reason = @event.Reason;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(UserEmailChangedDomainEvent @event)
    {
        Email = @event.NewEmail;
        UpdatedAt = @event.OccurredAt;
    }

    private void Apply(UserGenderChangedDomainEvent @event)
    {
        Gender = @event.NewGender;
        UpdatedAt = @event.OccurredAt;
    }
    #endregion

    #region Guard Clauses
    private Result EnsureNotDeleted()
    {
        if (IsDeleted)
            return UserErrors.Deleted;

        return Result.Success();
    }
    #endregion
}
