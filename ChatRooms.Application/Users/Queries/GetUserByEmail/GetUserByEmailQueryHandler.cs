using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Queries.GetUserByEmail;

public sealed class GetUserByEmailQueryHandler(IUserQuery query) : IQueryHandler<GetUserByEmailQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetByEmailAsync(request.Email, cancellationToken) ?? throw new KeyNotFoundException(nameof(request.Email));
        return dto;
    }
}
