using System.Security.Claims;

namespace DoAnLTW.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.GivenName)
               ?? principal.Identity?.Name
               ?? "Người dùng";
    }
}
