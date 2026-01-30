using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Users.Events;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    public Name Name { get; private set; }
    private User(UserId id, DateTimeUtc createdAt) : base(id, createdAt) { }
    public static User Create(Name name, DateTimeUtc occurredAt)
    {
        var user = new User(UserId.New(), occurredAt);
        user.Raise(new UserCreatedDomainEvent(user.Id, name, occurredAt));
        return user;
    }

    public override void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedDomainEvent e:
                Apply(e);
                break;
            default:
                throw new InvalidOperationException($"Event '{@event.GetType().Name}' is not supported by {nameof(User)}");
        }
    }

    #region Event Appliers
    private void Apply(UserCreatedDomainEvent @event)
    {
        Id = @event.UserId;
        Name = @event.Name;
    }
    #endregion
}
