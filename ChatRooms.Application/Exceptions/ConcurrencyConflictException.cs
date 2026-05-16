namespace ChatRooms.Application.Exceptions;

public sealed class ConcurrencyConflictException(string message) : InvalidOperationException(message)
{
}
