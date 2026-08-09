namespace ChatRooms.Domain.Shared.Errors;

public static class RoomParticipantErrors
{
    public static readonly Error NotTransient = new("RoomParticipant.NotTransient", "Only transient room members can be created.");
    public static readonly Error AlreadyJoined = new("RoomParticipant.AlreadyJoined", "User has already joined the room.");
    public static readonly Error NotExistInRoom = new("RoomParticipant.NotExistInRoom", "User is not a participant of the room.");
}
