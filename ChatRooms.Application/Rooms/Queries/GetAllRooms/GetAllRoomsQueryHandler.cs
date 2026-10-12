using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Rooms;

namespace ChatRooms.Application.Rooms.Queries.GetAllRooms;

public sealed class GetAllRoomsQueryHandler(IRoomQuery query)
    : IQueryHandler<GetAllRoomsQuery, IReadOnlyList<RoomListItem>>
{
    public async Task<IReadOnlyList<RoomListItem>> Handle(
        GetAllRoomsQuery request,
        CancellationToken cancellationToken)
    {
        return await query.GetAllAsync(cancellationToken);
    }
}
