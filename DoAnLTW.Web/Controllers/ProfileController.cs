using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Finance;
using DoAnLTW.Web.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

[Authorize(Roles = "User,Admin")]
public class ProfileController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly AvatarStorageService _avatarStorageService;
    private readonly AuditLogService _auditLogService;

    public ProfileController(
        FinanceDbContext db,
        PasswordService passwordService,
        AvatarStorageService avatarStorageService,
        AuditLogService auditLogService)
    {
        _db = db;
        _passwordService = passwordService;
        _avatarStorageService = avatarStorageService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstAsync(x => x.Id == CurrentUserId, cancellationToken);
        return View(BuildProfileViewModel(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstAsync(x => x.Id == CurrentUserId, cancellationToken);

        if (!ModelState.IsValid)
        {
            model.AvatarPath = user.AvatarPath;
            model.CreatedAt = user.CreatedAt;
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.Id != CurrentUserId && x.Email == normalizedEmail, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Email), "Email da duoc dung.");
            model.AvatarPath = user.AvatarPath;
            model.CreatedAt = user.CreatedAt;
            return View(model);
        }

        user.DisplayName = model.DisplayName.Trim();
        user.Email = normalizedEmail;
        user.AvatarPath = await _avatarStorageService.SaveAsync(model.AvatarFile, user.AvatarPath, cancellationToken);

        if (!string.IsNullOrWhiteSpace(model.CurrentPassword) || !string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (!_passwordService.Verify(model.CurrentPassword ?? string.Empty, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Mat khau hien tai khong dung.");
                model.AvatarPath = user.AvatarPath;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Vui long nhap mat khau moi.");
                model.AvatarPath = user.AvatarPath;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            user.PasswordHash = _passwordService.Hash(model.NewPassword);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _auditLogService.WriteAsync("ProfileUpdate", "Cap nhat ho so ca nhan", CurrentUserId, cancellationToken: cancellationToken);
        TempData["SuccessMessage"] = "Da cap nhat ho so.";
        return RedirectToAction(nameof(Index));
    }

    private static ProfileViewModel BuildProfileViewModel(DoAnLTW.Web.Models.Entities.AppUser user)
    {
        return new ProfileViewModel
        {
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarPath = user.AvatarPath,
            CreatedAt = user.CreatedAt
        };
    }
}
