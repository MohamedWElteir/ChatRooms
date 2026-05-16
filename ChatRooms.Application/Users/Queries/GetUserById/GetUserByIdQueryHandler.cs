using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.Users;
using ChatRooms.DTOs.Users;

namespace ChatRooms.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserQuery query) : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException(nameof(User));
        return dto;
    }
}
