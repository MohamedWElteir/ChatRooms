namespace ChatRooms.Infrastructure.BackgroundJobs.Projectors;

public interface IEventProjector
{
    Task ProjectAsync(string eventContent, CancellationToken cancellationToken);
}