namespace ChatRooms.Domain.Shared;

public abstract class Entity<TId>(TId id, DateTime dateTime) : IEquatable<Entity<TId>> where TId : struct, IEquatable<TId>
{
    public TId Id { get; private init; } = id;
    public DateTime CreatedAt { get; private init; } = dateTime;
    public DateTime UpdatedAt { get; protected set; } = dateTime;
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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


    protected void Raise(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

}
