using ChatRooms.Infrastructure.BackgroundJobs.Projectors;
using ChatRooms.Infrastructure.Persistence.Outbox;

namespace ChatRooms.Infrastructure.BackgroundJobs;

public interface IOutboxMessageProcessor
{
    Task ProcessAsync(OutboxMessage message, IEventProjector? projector, CancellationToken cancellationToken);
}
