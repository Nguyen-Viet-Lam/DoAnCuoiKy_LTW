using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

/// <summary>Controller quản lý danh sách ví, số dư ban đầu và trạng thái sử dụng của ví.</summary>
[Authorize(Roles = "User")]
public class WalletsController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly AuditLogService _auditLogService;
    private readonly WalletBalanceMonitorService _walletBalanceMonitorService;

    /// <summary>
    /// Khởi tạo lớp WalletsController và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public WalletsController(
        FinanceDbContext db,
        AuditLogService auditLogService,
        WalletBalanceMonitorService walletBalanceMonitorService)
    {
        _db = db;
        _auditLogService = auditLogService;
        _walletBalanceMonitorService = walletBalanceMonitorService;
    }

    /// <summary>
    /// Nạp danh sách ví và dữ liệu form để hiển thị trang quản lý ví.
    /// </summary>
    public async Task<IActionResult> Index(int? editId, CancellationToken cancellationToken)
    {
        var wallets = await _db.Wallets
            .Where(x => x.UserId == CurrentUserId && !x.IsArchived)
            .OrderByDescending(x => x.CurrentBalance)
            .Select(x => new WalletItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                CurrentBalance = x.CurrentBalance,
                Note = x.Note,
                TransactionCount = x.Transactions.Count,
                IsLowBalance = _walletBalanceMonitorService.IsLowBalance(x.CurrentBalance)
            })
            .ToListAsync(cancellationToken);

        var model = new WalletsPageViewModel
        {
            Wallets = wallets,
            TotalBalance = wallets.Sum(x => x.CurrentBalance),
            LowBalanceThreshold = WalletBalanceMonitorService.LowBalanceThreshold
        };

        if (editId.HasValue)
        {
            var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.Id == editId && x.UserId == CurrentUserId, cancellationToken);
            if (wallet is not null)
            {
                model.Form = new WalletFormViewModel
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Type = wallet.Type,
                    InitialBalance = wallet.InitialBalance,
                    Note = wallet.Note
                };
            }
        }

        return View(model);
    }

    /// <summary>
    /// Tạo ví mới hoặc cập nhật thông tin ví hiện có của người dùng.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Form")] WalletFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildPageModelAsync(model, cancellationToken));
        }

        if (model.Id.HasValue)
        {
            var existing = await _db.Wallets.FirstOrDefaultAsync(x => x.Id == model.Id && x.UserId == CurrentUserId, cancellationToken);
            if (existing is null)
            {
                return NotFound();
            }

            var initialBalanceDelta = model.InitialBalance - existing.InitialBalance;

            existing.Name = model.Name.Trim();
            existing.Type = model.Type;
            existing.InitialBalance = model.InitialBalance;
            existing.CurrentBalance += initialBalanceDelta;
            existing.Note = model.Note?.Trim();

            await _db.SaveChangesAsync(cancellationToken);
            await _auditLogService.WriteAsync("WalletUpdate", $"Cập nhật ví {existing.Name}", CurrentUserId, cancellationToken: cancellationToken);
            TempData["SuccessMessage"] = "Cập nhật ví thành công.";
        }
        else
        {
            var wallet = new Wallet
            {
                UserId = CurrentUserId,
                Name = model.Name.Trim(),
                Type = model.Type,
                InitialBalance = model.InitialBalance,
                CurrentBalance = model.InitialBalance,
                Note = model.Note?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync(cancellationToken);
            await _auditLogService.WriteAsync("WalletCreate", $"Tạo ví {wallet.Name}", CurrentUserId, cancellationToken: cancellationToken);
            TempData["SuccessMessage"] = "Tạo ví thành công.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Xóa ví khi ví đó chưa phát sinh giao dịch ràng buộc.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var wallet = await _db.Wallets
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId, cancellationToken);

        if (wallet is null)
        {
            return NotFound();
        }

        if (wallet.Transactions.Any())
        {
            TempData["ErrorMessage"] = "Ví đã có giao dịch nên không thể xóa. Bạn có thể giữ lại để theo dõi lịch sử.";
            return RedirectToAction(nameof(Index));
        }

        _db.Wallets.Remove(wallet);
        await _db.SaveChangesAsync(cancellationToken);
        await _auditLogService.WriteAsync("WalletDelete", $"Xóa ví {wallet.Name}", CurrentUserId, cancellationToken: cancellationToken);
        TempData["SuccessMessage"] = "Đã xóa ví.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<WalletsPageViewModel> BuildPageModelAsync(WalletFormViewModel form, CancellationToken cancellationToken)
    {
        var wallets = await _db.Wallets
            .Where(x => x.UserId == CurrentUserId && !x.IsArchived)
            .OrderByDescending(x => x.CurrentBalance)
            .Select(x => new WalletItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                CurrentBalance = x.CurrentBalance,
                Note = x.Note,
                TransactionCount = x.Transactions.Count,
                IsLowBalance = _walletBalanceMonitorService.IsLowBalance(x.CurrentBalance)
            })
            .ToListAsync(cancellationToken);

        return new WalletsPageViewModel
        {
            Form = form,
            Wallets = wallets,
            TotalBalance = wallets.Sum(x => x.CurrentBalance),
            LowBalanceThreshold = WalletBalanceMonitorService.LowBalanceThreshold
        };
    }
}
