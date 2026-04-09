using System.Security.Claims;

namespace DoAnLTW.Web.Extensions;

/// <summary>Extension helper để đọc nhanh thông tin người dùng từ ClaimsPrincipal.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Đọc claim định danh để lấy mã người dùng từ phiên đăng nhập hiện tại.
    /// </summary>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    /// <summary>
    /// Đọc claim để lấy tên hiển thị của người dùng hiện tại.
    /// </summary>
    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.GivenName)
               ?? principal.Identity?.Name
               ?? "Người dùng";
    }
}
