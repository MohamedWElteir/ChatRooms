using ChatRooms.Domain.Rooms.Appliers;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.Domain.Rooms.Events;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;
namespace ChatRooms.Domain.Rooms;

public sealed class Room : AggregateRoot<RoomId>
{
    public Name Name { get; set; }
    public Capacity Capacity { get; set; }
    public RoomCode Code { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Active;
    public DeletionReason? DeletionReason { get; set; }
    private static readonly Dictionary<Type, Action<Room, IDomainEvent>> _appliers = new()
    {
        {typeof(RoomCreatedDomainEvent), (room, @event) => RoomCreatedDomainEventApplier.Apply(room,(RoomCreatedDomainEvent)@event)},
        {typeof(RoomRenamedDomainEvent), (room, @event) => RoomRenamedDomainEventApplier.Apply(room,(RoomRenamedDomainEvent)@event)},
        {typeof(RoomDeletedDomainEvent), (room, @event) => RoomDeletedDomainEventApplier.Apply(room, (RoomDeletedDomainEvent)@event)},
        {typeof(RoomArchivedDomainEvent), (room, @event) => RoomArchivedDomainEventApplier.Apply(room, (RoomArchivedDomainEvent)@event)},
        {typeof(RoomCapacityChangedDomainEvent), (room, @event) => RoomCapacityChangedDomainEventApplier.Apply(room, (RoomCapacityChangedDomainEvent)@event)},
    };
    private Room(RoomId id, DateTime createdAt) : base(id, createdAt) { }
    public override void Apply(IDomainEvent @event)
    {
        if (!_appliers.TryGetValue(@event.GetType(), out var applier))
            throw new InvalidOperationException($"No applier registered for {@event.GetType().Name}");

        applier(this, @event);

    }
    public static Room Create(Name name, Capacity capacity, DateTime createdAt)
    {
        var room = new Room(RoomId.New(), createdAt);
        room.Raise(new RoomCreatedDomainEvent(room.Id, name, capacity, createdAt));
        return room;

    }
    public void Rename(Name newName)
    {
        if (Name == newName)
            return;
        Raise(new RoomRenamedDomainEvent(Id, newName, DateTime.UtcNow));
    }
    public void Archive(DateTime archivedAt)
    {
        if (Status != RoomStatus.Active)
            throw new InvalidOperationException("Only active rooms can be archived.");

        Raise(new RoomArchivedDomainEvent(Id, archivedAt));
    }

    public void Delete(DateTime deletedAt, DeletionReason reason)
    {
        if (Status == RoomStatus.Deleted)
            throw new InvalidOperationException("Can't delete a deleted room.");

        if (Status == RoomStatus.Active && reason == Enums.DeletionReason.Inactivity)
            throw new InvalidOperationException("Active rooms cannot be deleted due to inactivity.");

        Raise(new RoomDeletedDomainEvent(Id, reason, deletedAt));
    }

    public void ChangeCapacity(Capacity newCapacity)
    {
        if (Capacity == newCapacity)
            return;
        // TODO: Add validation logic for decreasing capacity based on current number of participants

        Raise(new RoomCapacityChangedDomainEvent(Id, newCapacity, DateTime.UtcNow));
    }
}
