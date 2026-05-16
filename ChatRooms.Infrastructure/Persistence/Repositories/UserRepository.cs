using ChatRooms.Application.Users.Commands;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;
using ChatRooms.Infrastructure.Persistence.DB.Write;
using Microsoft.EntityFrameworkCore;

namespace ChatRooms.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(WriteDbContext dbContext) : IUserRepository
{
    public async Task Add(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetByEmail(Email email, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetById(UserId id, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
