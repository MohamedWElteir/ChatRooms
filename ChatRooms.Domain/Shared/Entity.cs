using ChatRooms.Domain.Rooms.Enums;
namespace ChatRooms.Domain.Shared;

public abstract class Entity<TId>(TId id, DateTimeUtc dateTime) : IEquatable<Entity<TId>> where TId : struct, IEquatable<TId>
{
    public TId Id { get; internal set; } = id;
    public DateTimeUtc CreatedAt { get; private init; } = dateTime;
    public DateTimeUtc? UpdatedAt { get; internal set; }
    public DateTimeUtc? DeletedAt { get; internal set; }
    public DeletionReason? DeletionReason { get; internal set; }
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

}
