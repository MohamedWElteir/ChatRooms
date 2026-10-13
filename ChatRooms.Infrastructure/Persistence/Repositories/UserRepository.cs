using ChatRooms.Application.Users.Commands;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(WriteDbContext dbContext)
    : GenericRepository<User, UserId>(dbContext), IUserRepository
{
    public async Task<User?> GetByEmail(Email email, CancellationToken cancellationToken)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
