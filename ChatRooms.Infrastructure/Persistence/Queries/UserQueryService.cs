using ChatRooms.Application.Users.Queries;
using ChatRooms.DTOs.Users;
using ChatRooms.Infrastructure.Persistence.DB.Read;
using MongoDB.Driver;

namespace ChatRooms.Infrastructure.Persistence.Queries;

public sealed class UserQueryService(ReadDbContext readDbContext) : IUserQuery
{
    private readonly FilterDefinitionBuilder<UserDto> _userFilterDefinitionBuilder = Builders<UserDto>.Filter;

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = _userFilterDefinitionBuilder.Eq(u => u.Id, id);
        return await readDbContext.Users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = _userFilterDefinitionBuilder.Eq(u => u.Email, email);
        return await readDbContext.Users.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
