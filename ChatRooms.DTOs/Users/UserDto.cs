namespace ChatRooms.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Gender,
    int Version
    );