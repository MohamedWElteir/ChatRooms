using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Infrastructure.Persistence.Outbox;

public interface IOutboxMessageFactory
{
    IReadOnlyList<OutboxMessage> CreateOutboxMessages(IEnumerable<IAggregateRoot> aggregates);
}
