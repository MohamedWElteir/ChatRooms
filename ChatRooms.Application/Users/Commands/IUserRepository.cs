using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Application.Users.Commands;

public interface IUserRepository
{
    Task<User?> GetById(UserId id, CancellationToken cancellationToken);
    Task<User?> GetByEmail(Email email, CancellationToken cancellationToken);
    Task Add(User user, CancellationToken cancellationToken);
}
