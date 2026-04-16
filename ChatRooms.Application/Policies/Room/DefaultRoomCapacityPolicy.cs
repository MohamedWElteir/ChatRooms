using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Application.Policies.Room;

public sealed class DefaultRoomCapacityPolicy : IRoomCapacityPolicy
{
    public int MaxCapacity => Capacity.Max;
    public int MinCapacity => Capacity.Min;
}
