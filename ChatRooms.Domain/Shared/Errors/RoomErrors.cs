namespace ChatRooms.Domain.Shared.Errors;

public static class RoomErrors
{
    public static readonly Error NotFound = new("Room.NotFound", "Room not found.");
    public static readonly Error NotTransient = new("Room.NotTransient", "Only transient rooms can be created.");
    public static readonly Error Deleted = new("Room.Deleted", "Operation not allowed on deleted room.");
    public static readonly Error NotActive = new("Room.NotActive", "Operation only allowed on active rooms.");
    public static readonly Error CapacityReached = new("Room.CapacityReached", "Room capacity reached.");
    public static readonly Error NoParticipantsToLeave = new("Room.NoParticipantsToLeave", "No participants to leave.");
    public static readonly Error ActiveRoomCannotBeDeletedDueToInactivity = new("Room.ActiveRoomCannotBeDeletedDueToInactivity", "Active rooms cannot be deleted due to inactivity.");
    public static readonly Error NewCapacityCannotBeLessThanCurrentParticipants = new("Room.NewCapacityCannotBeLessThanCurrentParticipants", "New capacity cannot be less than current participants count.");
    public static readonly Error OnlyArchivedCanBeRestored = new("Room.OnlyArchivedCanBeRestored", "Only archived rooms can be restored.");
    public static readonly Error InvalidDeletionReason = new ("Room.InvalidDeletionReason", "Invalid deletion reason.")
}
