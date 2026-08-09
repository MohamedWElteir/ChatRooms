namespace ChatRooms.Domain.RoomParticipants.ValueObjects;

public readonly record struct RoomParticipantId
{
    public readonly Guid Value;
    private RoomParticipantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoomParticipantId cannot be empty.");
        Value = value;
    }

    public static RoomParticipantId New() => new(Guid.NewGuid());
    public static RoomParticipantId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
    public static implicit operator Guid(RoomParticipantId roomMemberId) => roomMemberId.Value;
    public static implicit operator RoomParticipantId(Guid roomMemberId) => From(roomMemberId);
}
