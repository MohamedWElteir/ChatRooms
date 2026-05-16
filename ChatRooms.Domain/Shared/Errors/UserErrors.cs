namespace ChatRooms.Domain.Shared.Errors;

public static class UserErrors
{
    public static readonly Error AlreadyDeleted = new("User.AlreadyDeleted", "User is already deleted.");
}
