using ChatRooms.SharedKernel.Utils;

namespace ChatRooms.Domain.Shared;

public abstract class Entity<TId>(TId id, DateTime dateTime) : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; private init; } = id ?? throw new ArgumentNullException(nameof(id));
    public DateTime CreatedAt { get; private init; } = dateTime;
    public DateTime UpdatedAt { get; set; } = dateTime;
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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

    protected void Raise(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

}
