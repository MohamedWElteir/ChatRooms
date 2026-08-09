using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Shared.Contracts;

namespace ChatRooms.Application.Abstractions.Persistence;

public interface IRepository<TAggregateRoot, TId>
    where TAggregateRoot : AggregateRoot<TId>, IAggregateRoot
    where TId : struct, IEquatable<TId>
{
    Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken);

    Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken);

}