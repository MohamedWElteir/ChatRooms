using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.Repositories;

public sealed class RoomRepository(WriteDbContext dbContext)
    : GenericRepository<Room, RoomId>(dbContext), IRoomRepository
{
    public async Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken)
    {
        return await DbSet.AnyAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }
}