using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Rooms.DTOs;
using ChatRooms.Domain.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetRoomById;

public sealed class GetRoomByIdQueryHandler(IRoomQuery query) : IQueryHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetById(request.Id, cancellationToken) ?? throw new KeyNotFoundException(nameof(Room));
        return dto;
    }
}
