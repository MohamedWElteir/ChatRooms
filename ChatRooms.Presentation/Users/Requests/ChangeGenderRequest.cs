using ChatRooms.Domain.Users.Enums;

namespace ChatRooms.Presentation.Users.Requests;

public sealed record ChangeGenderRequest(Gender NewGender);
