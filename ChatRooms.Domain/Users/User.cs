using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Shared.Enums;
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
    public static User Create(Name name, Gender gender, BirthDate birthDate)
    {
        var user = new User();
        user.Raise(new UserCreatedDomainEvent(UserId.New(), name, gender, birthDate));
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
            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(User)}");
        }
    }
    public void Rename(Name newName)
    {
        if (Name == newName)
            return;
        Raise(new UserRenamedDomainEvent(Id, newName));
    }
    public void Delete(DeletionReason reason)
    {
        if (IsDeleted)
            throw new InvalidOperationException("User is already deleted.");
        Raise(new UserDeletedDomainEvent(Id, reason));
    }
    #region Event Appliers
    private void Apply(UserCreatedDomainEvent @event)
    {
        Id = @event.UserId;
        Name = @event.Name;
        Gender = @event.Gender;
        BirthDate = @event.BirthDate;
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
    }
    #endregion
}
