using ChatRooms.Domain.Users.Enums;

namespace ChatRooms.Presentation.Users.Requests;

public sealed record RegisterRequest(string Name, string Email, Gender Gender, DateTime BirthDate);
