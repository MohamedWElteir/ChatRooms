using ChatRooms.Domain.Shared.Contracts;
namespace ChatRooms.Domain.Shared;

public abstract class Entity<TId> : IAuditable, IEquatable<Entity<TId>> where TId : struct, IEquatable<TId>
{
    public TId Id { get; internal set; }
    public DateTimeUtc CreatedAt { get; protected set; }
    public DateTimeUtc? UpdatedAt { get; internal set; }

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
    public bool IsTransient() => EqualityComparer<TId>.Default.Equals(Id, default);

}
