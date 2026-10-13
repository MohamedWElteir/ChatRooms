namespace ChatRooms.Application.Abstractions.Persistence;

public interface IRepository<TEntity, in TId> where TEntity : class
{
    Task<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetOrDefaultAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);
}
