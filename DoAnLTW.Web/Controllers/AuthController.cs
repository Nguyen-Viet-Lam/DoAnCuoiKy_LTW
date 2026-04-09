using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Auth;
using DoAnLTW.Web.Services.Finance;
using DoAnLTW.Web.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

/// <summary>Controller xử lý đăng nhập, đăng ký, xác thực OTP, quên mật khẩu và đăng xuất.</summary>
public class AuthController : Controller
{
    private readonly FinanceDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly OtpService _otpService;
    private readonly CookieAuthService _cookieAuthService;
    private readonly AuditLogService _auditLogService;

    /// <summary>
    /// Khởi tạo lớp AuthController và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public AuthController(
        FinanceDbContext db,
        PasswordService passwordService,
        OtpService otpService,
        CookieAuthService cookieAuthService,
        AuditLogService auditLogService)
    {
        _db = db;
        _passwordService = passwordService;
        _otpService = otpService;
        _cookieAuthService = cookieAuthService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Hiển thị form đăng nhập hoặc xác thực thông tin đăng nhập rồi tạo cookie xác thực.
    /// </summary>
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Hiển thị form đăng nhập hoặc xác thực thông tin đăng nhập rồi tạo cookie xác thực.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null || !_passwordService.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Email hoac mat khau chua dung.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tai khoan dang bi khoa.");
            return View(model);
        }

        if (!user.IsEmailVerified)
        {
            ModelState.AddModelError(string.Empty, "Ban can xac thuc OTP truoc khi dang nhap.");
            ViewBag.PendingEmail = user.Email;
            return View(model);
        }

        await _cookieAuthService.SignInAsync(HttpContext, user, model.RememberMe);
        return RedirectToLocal(model.ReturnUrl, user.Role?.Name);
    }

    /// <summary>
    /// Hiển thị form đăng ký hoặc tạo tài khoản mới rồi gửi OTP xác thực email.
    /// </summary>
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    /// <summary>
    /// Hiển thị form đăng ký hoặc tạo tài khoản mới rồi gửi OTP xác thực email.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var normalizedUsername = model.Username.Trim();

        if (await _db.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Email), "Email da ton tai.");
        }

        if (await _db.Users.AnyAsync(x => x.Username == normalizedUsername, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Username), "Ten dang nhap da ton tai.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var roleId = await _db.Roles.Where(x => x.Name == "User").Select(x => x.Id).FirstAsync(cancellationToken);
        var user = new AppUser
        {
            Username = normalizedUsername,
            DisplayName = model.DisplayName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordService.Hash(model.Password),
            RoleId = roleId,
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await _otpService.SendOtpAsync(user, user.Email, "Register", cancellationToken);
        await _auditLogService.WriteAsync("Register", $"Dang ky: {user.Email}", user.Id, cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = "Dang ky thanh cong. Vui long nhap OTP da gui ve Gmail.";
        return RedirectToAction(nameof(VerifyOtp), new { email = user.Email, purpose = "Register" });
    }

    /// <summary>
    /// Hiển thị màn hình nhập OTP hoặc kiểm tra OTP người dùng vừa nhập.
    /// </summary>
    [AllowAnonymous]
    public IActionResult VerifyOtp(string? email = null, string purpose = "Register")
    {
        return View(new VerifyOtpViewModel
        {
            Email = email ?? string.Empty,
            Purpose = purpose
        });
    }

    /// <summary>
    /// Hiển thị màn hình nhập OTP hoặc kiểm tra OTP người dùng vừa nhập.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _otpService.VerifyAsync(model.Email, model.Purpose, model.Code, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        if (model.Purpose == "Register")
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email.Trim().ToLowerInvariant(), cancellationToken);
            if (user is not null)
            {
                user.IsEmailVerified = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        TempData["SuccessMessage"] = "Xac thuc OTP thanh cong. Ban co the dang nhap.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Phát sinh và gửi lại OTP mới cho email và mục đích xác thực hiện tại.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email, string purpose = "Register", CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        await _otpService.SendOtpAsync(user, normalizedEmail, purpose, cancellationToken);
        TempData["SuccessMessage"] = "Da gui lai OTP.";
        return RedirectToAction(nameof(VerifyOtp), new { email = normalizedEmail, purpose });
    }

    /// <summary>
    /// Hiển thị form quên mật khẩu hoặc gửi OTP hỗ trợ đặt lại mật khẩu.
    /// </summary>
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    /// <summary>
    /// Hiển thị form quên mật khẩu hoặc gửi OTP hỗ trợ đặt lại mật khẩu.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is not null)
        {
            await _otpService.SendOtpAsync(user, email, "ResetPassword", cancellationToken);
        }

        TempData["SuccessMessage"] = "Neu email ton tai, he thong da gui OTP doi mat khau.";
        return RedirectToAction(nameof(ResetPassword), new { email });
    }

    /// <summary>
    /// Hiển thị form đặt lại mật khẩu hoặc cập nhật mật khẩu mới sau khi OTP hợp lệ.
    /// </summary>
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null)
    {
        return View(new ResetPasswordViewModel { Email = email ?? string.Empty });
    }

    /// <summary>
    /// Hiển thị form đặt lại mật khẩu hoặc cập nhật mật khẩu mới sau khi OTP hợp lệ.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var otpResult = await _otpService.VerifyAsync(model.Email, "ResetPassword", model.Code, cancellationToken);
        if (!otpResult.Success)
        {
            ModelState.AddModelError(string.Empty, otpResult.Message);
            return View(model);
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Khong tim thay tai khoan.");
            return View(model);
        }

        user.PasswordHash = _passwordService.Hash(model.NewPassword);
        await _db.SaveChangesAsync(cancellationToken);
        await _auditLogService.WriteAsync("ResetPassword", $"Doi mat khau: {user.Email}", user.Id, cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = "Doi mat khau thanh cong.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Đăng xuất người dùng bằng cách xóa cookie xác thực hiện tại.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _cookieAuthService.SignOutAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Hiển thị trang thông báo khi người dùng không đủ quyền truy cập tài nguyên.
    /// </summary>
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl, string? signedInRole = null)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var isAdmin = string.Equals(signedInRole, "Admin", StringComparison.OrdinalIgnoreCase) || User.IsInRole("Admin");

        return isAdmin
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Dashboard");
    }
}
