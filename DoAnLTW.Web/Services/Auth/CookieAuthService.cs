using System.Security.Claims;
using DoAnLTW.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DoAnLTW.Web.Services.Auth;

/// <summary>Service tạo và hủy cookie xác thực cho người dùng sau khi đăng nhập hoặc đăng xuất.</summary>
public class CookieAuthService
{
    /// <summary>
    /// Tạo claims và ghi cookie xác thực khi người dùng đăng nhập thành công.
    /// </summary>
    public async Task SignInAsync(HttpContext httpContext, AppUser user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 14 : 1)
            });
    }

    /// <summary>
    /// Xóa cookie xác thực để kết thúc phiên đăng nhập hiện tại.
    /// </summary>
    public Task SignOutAsync(HttpContext httpContext)
    {
        return httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
