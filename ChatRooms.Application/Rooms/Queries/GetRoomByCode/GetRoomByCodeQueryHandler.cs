using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetRoomByCode;

public sealed class GetRoomByCodeQueryHandler(IRoomQuery query) : IQueryHandler<GetRoomByCodeQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByCodeQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetByCodeAsync(request.Code, cancellationToken) ?? throw new KeyNotFoundException(nameof(request.Code));
        return dto;
    }
}
