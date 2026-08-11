using ChatRooms.Domain.RoomParticipants.Events;
using ChatRooms.Domain.RoomParticipants.ValueObjects;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
using ChatRooms.Domain.Shared.Errors;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Domain.RoomParticipants;

public sealed class RoomParticipant : AggregateRoot<RoomParticipantId>
{
    public RoomId RoomId { get; private set; }
    public UserId UserId { get; private set; }
    public DateTimeUtc JoinedAt { get; private set; }
    public DateTimeUtc? LeftAt { get; private set; }

    private RoomParticipant() { }
    public override void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case RoomParticipantCreatedDomainEvent e:
                Apply(e);
                break;
            case RoomParticipantLeftDomainEvent e: 
                Apply(e); 
                break;
        }
    }

    public static Result<RoomParticipant> Create(RoomId roomId, UserId userId, DateTimeUtc joinedAt)
    {
        var roomMember = new RoomParticipant();
        if (!roomMember.IsTransient())
            return RoomParticipantErrors.NotTransient;

        roomMember.Raise(new RoomParticipantCreatedDomainEvent(
            RoomParticipantId.New(),
            roomId,
            userId,
            joinedAt));
        return roomMember;
    }

    public Result Leave(DateTimeUtc leftAt)
    {
        if (LeftAt.HasValue)
            return RoomParticipantErrors.AlreadyLeft;
        Raise(new RoomParticipantLeftDomainEvent(Id, RoomId, UserId, leftAt));
        return Result.Success();
    }

    #region Event Appliers
    private void Apply(RoomParticipantCreatedDomainEvent @event)
    {
        Id = @event.RoomMemberId;
        RoomId = @event.RoomId;
        UserId = @event.UserId;
        JoinedAt = @event.JoinedAt;
    }

    private void Apply(RoomParticipantLeftDomainEvent @event)
    {
        LeftAt = @event.LeftAt;
    }
    #endregion
}
