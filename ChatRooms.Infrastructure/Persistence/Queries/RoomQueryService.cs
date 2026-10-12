using ChatRooms.Application.Rooms.Queries;
using ChatRooms.Domain.Rooms.Enums;
using ChatRooms.DTOs.Rooms;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using MongoDB.Driver;

namespace ChatRooms.Infrastructure.Persistence.Queries;

public sealed class RoomQueryService(ReadDbContext readDbContext) : IRoomQuery
{
    private readonly FilterDefinitionBuilder<RoomDto> _roomFilterDefinitionBuilder = Builders<RoomDto>.Filter;

    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = _roomFilterDefinitionBuilder.And(
            _roomFilterDefinitionBuilder.Eq(r => r.Id, id),
            _roomFilterDefinitionBuilder.Ne(r => r.Status, nameof(RoomStatus.Deleted)));

        return await readDbContext.Rooms.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RoomDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var filter = _roomFilterDefinitionBuilder.And(
            _roomFilterDefinitionBuilder.Eq(r => r.Code, code),
            _roomFilterDefinitionBuilder.Ne(r => r.Status, nameof(RoomStatus.Deleted)));

        return await readDbContext.Rooms.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoomListItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        var filter = _roomFilterDefinitionBuilder.Ne(r => r.Status, nameof(RoomStatus.Deleted));

        return await readDbContext.Rooms
            .Find(filter)
            .Project(r => new RoomListItem(r.Id, r.Name, r.Code, r.Capacity, r.CurrentParticipantsCount))
            .ToListAsync(cancellationToken);
    }
}