using ChatRooms.Domain.Rooms.Enums;

namespace ChatRooms.Domain.Shared.Contracts;

public interface ISoftDeletable
{
    public bool IsDeleted { get; }
    public DateTimeUtc? DeletedAt { get; }
    public DeletionReason? Reason { get; }
}
