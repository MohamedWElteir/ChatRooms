namespace ChatRooms.Application.Abstractions.Common;

public interface IGenerator<out T> where T : struct
{
    T Generate();
}