using ChatRooms.Domain.Rooms.Contracts;
using ChatRooms.Domain.Rooms.ValueObjects;

namespace ChatRooms.Infrastructure.Services;

public sealed class DefaultRoomCapacityPolicy : IRoomCapacityPolicy
{
    public int MaxCapacity => Capacity.Max;
    public int MinCapacity => Capacity.Min;
}
