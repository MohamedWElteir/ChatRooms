using System.Security.Claims;

namespace ChatRooms.SharedKernel.Extensions;

public static class Extensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
}
