using ChatRooms.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.Repositories;

public abstract class GenericRepository<TEntity, TId>(DbContext dbContext) : IRepository<TEntity, TId>
    where TEntity : class
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<TEntity> DbSet = dbContext.Set<TEntity>();

    public virtual async Task<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetOrDefaultAsync(id, cancellationToken);
        return entity ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with ID '{id}' was not found.");
    }

    public virtual async Task<TEntity?> GetOrDefaultAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}
