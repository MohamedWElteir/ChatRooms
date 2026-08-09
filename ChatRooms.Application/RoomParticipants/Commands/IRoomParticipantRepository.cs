using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.RoomParticipants;
using ChatRooms.Domain.RoomParticipants.ValueObjects;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Application.RoomParticipants.Commands;

public interface IRoomParticipantRepository : IRepository<RoomParticipant, RoomParticipantId>
{
    Task<bool> ExistsAsync(RoomId roomId, UserId userId, CancellationToken cancellationToken);
}
