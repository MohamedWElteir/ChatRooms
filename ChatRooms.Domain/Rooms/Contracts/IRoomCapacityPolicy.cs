using System;
using System.Collections.Generic;
using System.Text;

namespace ChatRooms.Domain.Rooms.Contracts;

public interface IRoomCapacityPolicy
{
    int MaxCapacity { get; }
    int MinCapacity { get; }
}
