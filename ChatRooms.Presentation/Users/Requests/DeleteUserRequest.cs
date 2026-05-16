using ChatRooms.Domain.Shared.Enums;

namespace ChatRooms.Presentation.Users.Requests;

public sealed record DeleteUserRequest(DeletionReason Reason);
