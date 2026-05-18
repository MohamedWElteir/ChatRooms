namespace ChatRooms.BFF.Services;

public sealed record RegisterBffRequest(
    string Name,
    string Email,
    string Password,
    string Gender,
    DateTime BirthDate);