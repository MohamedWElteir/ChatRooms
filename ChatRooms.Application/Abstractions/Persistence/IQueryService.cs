namespace ChatRooms.Application.Abstractions.Persistence;

public interface IQueryService<TResponse>
{
    Task<TResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
