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
    public static User Create(Name name, Email email, Gender gender, BirthDate birthDate, DateTimeUtc OccurredAt)
    {
        var user = new User();
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
    public void Rename(Name newName, DateTimeUtc occurredAt)
    {
        if (Name == newName)
            return;
        Raise(new UserRenamedDomainEvent(Id, newName, occurredAt));
    }
    public Result Delete(DeletionReason reason, DateTimeUtc occurredAt)
    {
        if (IsDeleted)
            return UserErrors.AlreadyDeleted;

        Raise(new UserDeletedDomainEvent(Id, reason, occurredAt));
        return Result.Success();
    }
    public void ChangeEmail(Email newEmail, DateTimeUtc occurredAt)
    {
        if (Email == newEmail)
            return;
        Raise(new UserEmailChangedDomainEvent(Id, newEmail, occurredAt));
    }

    public void ChangeGender(Gender newGender, DateTimeUtc occurredAt)
    {
        if (Gender == newGender)
            return;
        Raise(new UserGenderChangedDomainEvent(Id, newGender, occurredAt));
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
}
