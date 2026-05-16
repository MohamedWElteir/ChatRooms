namespace ChatRooms.Domain.Shared.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User not found.");
    public static readonly Error NotTransient = new("User.NotTransient", "Only transient users can be created.");
    public static readonly Error Deleted = new("User.Deleted", "Operation not allowed on deleted user.");
    public static readonly Error AlreadyDeleted = new("User.AlreadyDeleted", "User is already deleted.");
}
