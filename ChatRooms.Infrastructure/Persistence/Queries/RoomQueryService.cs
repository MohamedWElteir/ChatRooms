using ChatRooms.Application.Rooms.DTOs;
using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Infrastructure.Persistence.Read;
using MongoDB.Driver;

namespace ChatRooms.Infrastructure.Persistence.Queries;

public sealed class RoomQueryService(ReadDbContext readDbContext) : IRoomQuery
{
    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        FilterDefinition<RoomDto> filter = Builders<RoomDto>.Filter.Eq(r => r.Id, id);

        return await readDbContext.Rooms.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RoomDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        FilterDefinition<RoomDto> filter = Builders<RoomDto>.Filter.Eq(r => r.Code, code);

        return await readDbContext.Rooms.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoomListItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await readDbContext.Rooms
            .Find(_ => true)
            .Project(r => new RoomListItemDto(r.Id, r.Name, r.Code, r.Capacity, r.CurrentParticipantsCount))
            .ToListAsync(cancellationToken);
    }
}