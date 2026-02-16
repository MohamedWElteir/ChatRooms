using System.Security.Claims;

namespace ChatRooms.SharedKernel.Extensions;

public static class Extensions
{
    public static string GetUserId(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    public static bool IsNullOrWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str);
    public static long ToUnixMilliseconds(this DateTime dt) => new DateTimeOffset(dt).ToUnixTimeMilliseconds();
}
