
namespace ChatRooms.Application.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Gender
    );