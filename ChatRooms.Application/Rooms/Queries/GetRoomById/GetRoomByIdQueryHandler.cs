using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Rooms;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetRoomById;

public sealed class GetRoomByIdQueryHandler(IRoomQuery query) : IQueryHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException(nameof(Room));
        return dto;
    }
}
