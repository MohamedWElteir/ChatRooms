using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;

namespace ChatRooms.Application.Users.Commands;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> GetByEmail(Email email, CancellationToken cancellationToken);
}
