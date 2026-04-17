namespace ChatRooms.Application.Abstractions.Common;

public interface IGenerator<T> where T : struct
{
    T Generate();
}