using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Domain.Rooms.Contracts;

public interface IRoomCodeGenerator
{
    RoomCode Generate();
}
