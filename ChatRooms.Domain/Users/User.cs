using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    public Name Name { get; private set; }
    private User() : base() { }
    public static User Create(Name name)
    {
        var user = new User();
        user.Raise(new UserCreatedDomainEvent(UserId.New(), name));
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

    #region Event Appliers
    private void Apply(UserCreatedDomainEvent @event)
    {
        Id = @event.UserId;
        Name = @event.Name;
    }

    private void Apply(UserRenamedDomainEvent @event)
    {
        Name = @event.NewName;
    }
    #endregion
}
