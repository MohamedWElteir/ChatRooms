using ChatRooms.Application.Rooms.Commands;
using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.Repositories;

public sealed class RoomRepository(WriteDbContext dbContext) : IRoomRepository
{
    public async Task AddAsync(Room room, CancellationToken cancellationToken)
    {
        await dbContext.Rooms.AddAsync(room, cancellationToken);
    }

    public async Task<bool> ExistsAsync(RoomCode code, CancellationToken cancellationToken)
    {
        return await dbContext.Rooms.AnyAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<Room?> GetByCodeAsync(RoomCode code, CancellationToken cancellationToken)
    {
        return await dbContext.Rooms.FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<Room?> GetByIdAsync(RoomId id, CancellationToken cancellationToken)
    {
        return await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}