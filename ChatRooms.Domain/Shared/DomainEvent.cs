using ChatRooms.SharedKernel.Utils;

namespace ChatRooms.Domain.Shared;

public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; }

    protected DomainEvent(DateTime OccurredOn)
    {
        this.OccurredOn = OccurredOn;
    }

}
