using ChatRooms.Domain.Shared.Contracts;
using System.Text.Json;

namespace ChatRooms.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageFactory(JsonSerializerOptions jsonOptions) : IOutboxMessageFactory
{
    public IReadOnlyList<OutboxMessage> CreateOutboxMessages(IEnumerable<IAggregateRoot> aggregates)
    {
        var messages = new List<OutboxMessage>();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var outboxMessage = OutboxMessage.Create(
                    type: domainEvent.GetType().Name!,
                    content: JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), jsonOptions),
                    occurredOn: domainEvent.OccurredAt
                );
                messages.Add(outboxMessage);
            }
            aggregate.ClearDomainEvents();
        }

        return messages;
    }
}
