using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Queries;

public interface IUserQuery
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
