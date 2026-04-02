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

public class AuthController : Controller
{
    private readonly FinanceDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly OtpService _otpService;
    private readonly CookieAuthService _cookieAuthService;
    private readonly AuditLogService _auditLogService;

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

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

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

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

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

    [AllowAnonymous]
    public IActionResult VerifyOtp(string? email = null, string purpose = "Register")
    {
        return View(new VerifyOtpViewModel
        {
            Email = email ?? string.Empty,
            Purpose = purpose
        });
    }

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

    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

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

    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null)
    {
        return View(new ResetPasswordViewModel { Email = email ?? string.Empty });
    }

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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _cookieAuthService.SignOutAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }

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
