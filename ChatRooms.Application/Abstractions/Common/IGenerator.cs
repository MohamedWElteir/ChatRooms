namespace ChatRooms.Domain.Shared.Contracts;

public interface IGenerator<T> where T : struct
{
    T Generate();
}