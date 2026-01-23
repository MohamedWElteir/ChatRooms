namespace ChatRooms.Domain.Shared;

public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; private init; } = id ?? throw new ArgumentNullException(nameof(id));
    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null || right is null)
            return false;
        if (!ReferenceEquals(left, right))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (obj.GetType() != GetType())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (!ReferenceEquals(this, other))
            return false;

        return Equals((object)other);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

}
