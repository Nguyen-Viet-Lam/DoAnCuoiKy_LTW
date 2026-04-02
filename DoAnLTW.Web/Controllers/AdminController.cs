using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly AuditLogService _auditLogService;

    public AdminController(FinanceDbContext db, AuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var sevenDaysAgo = today.AddDays(-6);

        var latestUsers = await _db.Users
            .Include(x => x.Role)
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .Select(x => new AdminUserItemViewModel
            {
                Id = x.Id,
                Username = x.Username,
                DisplayName = x.DisplayName,
                Email = x.Email,
                RoleName = x.Role.Name,
                IsActive = x.IsActive,
                IsEmailVerified = x.IsEmailVerified,
                CreatedAt = x.CreatedAt,
                WalletCount = x.Wallets.Count,
                TransactionCount = x.Transactions.Count,
                TotalBalance = x.Wallets.Sum(w => w.CurrentBalance)
            })
            .ToListAsync(cancellationToken);

        var latestLogs = await BuildLogQuery()
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        var dailyStats = Enumerable.Range(0, 7)
            .Select(offset => sevenDaysAgo.AddDays(offset))
            .Select(day => new AdminDailyStatViewModel
            {
                Label = day.ToString("dd/MM"),
                NewUsers = _db.Users.Count(x => x.CreatedAt.Date == day),
                Transactions = _db.Transactions.Count(x => x.CreatedAt.Date == day)
            })
            .ToList();

        var model = new AdminDashboardPageViewModel
        {
            TotalUsers = await _db.Users.CountAsync(cancellationToken),
            ActiveUsers = await _db.Users.CountAsync(x => x.IsActive, cancellationToken),
            TotalTransactions = await _db.Transactions.CountAsync(cancellationToken),
            NewUsersToday = await _db.Users.CountAsync(x => x.CreatedAt.Date == today, cancellationToken),
            TotalCategories = await _db.Categories.CountAsync(cancellationToken),
            DailyStats = dailyStats,
            LatestUsers = latestUsers,
            LatestLogs = latestLogs
        };

        return View(model);
    }

    public async Task<IActionResult> Users(
        string searchTerm = "",
        string statusFilter = "All",
        string roleFilter = "All",
        CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .Include(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Username.ToLower().Contains(keyword) ||
                x.DisplayName.ToLower().Contains(keyword) ||
                x.Email.ToLower().Contains(keyword));
        }

        if (statusFilter == "Active")
        {
            query = query.Where(x => x.IsActive);
        }
        else if (statusFilter == "Locked")
        {
            query = query.Where(x => !x.IsActive);
        }

        if (roleFilter == "Admin" || roleFilter == "User")
        {
            query = query.Where(x => x.Role.Name == roleFilter);
        }

        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminUserItemViewModel
            {
                Id = x.Id,
                Username = x.Username,
                DisplayName = x.DisplayName,
                Email = x.Email,
                RoleName = x.Role.Name,
                IsActive = x.IsActive,
                IsEmailVerified = x.IsEmailVerified,
                CreatedAt = x.CreatedAt,
                WalletCount = x.Wallets.Count,
                TransactionCount = x.Transactions.Count,
                TotalBalance = x.Wallets.Sum(w => w.CurrentBalance)
            })
            .ToListAsync(cancellationToken);

        var model = new AdminUsersPageViewModel
        {
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            RoleFilter = roleFilter,
            TotalUsers = users.Count,
            ActiveUsers = users.Count(x => x.IsActive),
            LockedUsers = users.Count(x => !x.IsActive),
            Users = users
        };

        return View(model);
    }

    public async Task<IActionResult> Categories(
        string transactionTypeFilter = "All",
        int? editId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Categories.AsQueryable();
        if (transactionTypeFilter == "Expense" || transactionTypeFilter == "Income")
        {
            query = query.Where(x => x.TransactionType == transactionTypeFilter);
        }

        var categories = await query
            .OrderBy(x => x.TransactionType)
            .ThenBy(x => x.Name)
            .Select(x => new AdminCategoryItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                TransactionType = x.TransactionType,
                Icon = x.Icon,
                ColorHex = x.ColorHex,
                IsDefault = x.IsDefault,
                TransactionCount = x.Transactions.Count,
                BudgetCount = x.Budgets.Count
            })
            .ToListAsync(cancellationToken);

        var model = new AdminCategoriesPageViewModel
        {
            TransactionTypeFilter = transactionTypeFilter,
            TotalExpenseCategories = await _db.Categories.CountAsync(x => x.TransactionType == "Expense", cancellationToken),
            TotalIncomeCategories = await _db.Categories.CountAsync(x => x.TransactionType == "Income", cancellationToken),
            Categories = categories
        };

        if (editId.HasValue)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == editId.Value, cancellationToken);
            if (category is not null)
            {
                model.CategoryForm = new AdminCategoryFormViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    TransactionType = category.TransactionType,
                    Icon = category.Icon,
                    ColorHex = category.ColorHex
                };
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logs(string levelFilter = "All", string searchTerm = "", CancellationToken cancellationToken = default)
    {
        var query = BuildLogQuery();

        if (levelFilter is "Info" or "Warning" or "Error")
        {
            query = query.Where(x => x.Level == levelFilter);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Action.ToLower().Contains(keyword) ||
                x.Message.ToLower().Contains(keyword) ||
                (x.UserDisplayName != null && x.UserDisplayName.ToLower().Contains(keyword)));
        }

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var model = new AdminLogsPageViewModel
        {
            LevelFilter = levelFilter,
            SearchTerm = searchTerm,
            TotalLogs = await _db.SystemLogs.CountAsync(cancellationToken),
            ErrorLogs = await _db.SystemLogs.CountAsync(x => x.Level == "Error", cancellationToken),
            UserActionLogs = await _db.SystemLogs.CountAsync(x => x.UserId != null, cancellationToken),
            Logs = logs
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(int id, bool isActive, CancellationToken cancellationToken)
    {
        var user = await _db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Role.Name == "Admin")
        {
            TempData["ErrorMessage"] = "Khong khoa tai khoan admin bang man hinh nay.";
            return RedirectToAction(nameof(Users));
        }

        user.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(
            "AdminToggleUser",
            $"Doi trang thai nguoi dung #{id} = {isActive}",
            CurrentUserId,
            cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = "Da cap nhat trang thai nguoi dung.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory([Bind(Prefix = "CategoryForm")] AdminCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Du lieu danh muc chua hop le.";
            return RedirectToAction(nameof(Categories));
        }

        var exists = await _db.Categories.AnyAsync(
            x => x.Name == model.Name.Trim() &&
                 x.TransactionType == model.TransactionType &&
                 x.Id != model.Id,
            cancellationToken);

        if (exists)
        {
            TempData["ErrorMessage"] = "Danh muc da ton tai.";
            return RedirectToAction(nameof(Categories), new { transactionTypeFilter = model.TransactionType });
        }

        if (model.Id.HasValue)
        {
            var category = await _db.Categories
                .Include(x => x.Transactions)
                .Include(x => x.Budgets)
                .FirstOrDefaultAsync(x => x.Id == model.Id.Value, cancellationToken);

            if (category is null)
            {
                return NotFound();
            }

            var hasUsage = category.Transactions.Any() || category.Budgets.Any();
            if (hasUsage && !string.Equals(category.TransactionType, model.TransactionType, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Danh muc da co giao dich hoac ngan sach nen khong doi duoc loai.";
                return RedirectToAction(nameof(Categories), new { transactionTypeFilter = category.TransactionType, editId = category.Id });
            }

            category.Name = model.Name.Trim();
            category.TransactionType = model.TransactionType;
            category.Icon = model.Icon.Trim();
            category.ColorHex = model.ColorHex.Trim();

            await _db.SaveChangesAsync(cancellationToken);

            await _auditLogService.WriteAsync(
                "AdminCategoryUpdate",
                $"Cap nhat danh muc {model.Name}",
                CurrentUserId,
                cancellationToken: cancellationToken);

            TempData["SuccessMessage"] = "Da cap nhat danh muc.";
            return RedirectToAction(nameof(Categories), new { transactionTypeFilter = model.TransactionType });
        }

        _db.Categories.Add(new Category
        {
            Name = model.Name.Trim(),
            TransactionType = model.TransactionType,
            Icon = model.Icon.Trim(),
            ColorHex = model.ColorHex.Trim(),
            IsDefault = true,
            CreatedByUserId = CurrentUserId
        });

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(
            "AdminCategoryCreate",
            $"Them danh muc {model.Name}",
            CurrentUserId,
            cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = "Da them danh muc mac dinh.";
        return RedirectToAction(nameof(Categories), new { transactionTypeFilter = model.TransactionType });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, string transactionTypeFilter = "All", CancellationToken cancellationToken = default)
    {
        var category = await _db.Categories
            .Include(x => x.Transactions)
            .Include(x => x.Budgets)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        if (category.Transactions.Any() || category.Budgets.Any())
        {
            TempData["ErrorMessage"] = "Danh muc da duoc su dung nen khong the xoa.";
            return RedirectToAction(nameof(Categories), new { transactionTypeFilter });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(
            "AdminCategoryDelete",
            $"Xoa danh muc {category.Name}",
            CurrentUserId,
            cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = "Da xoa danh muc.";
        return RedirectToAction(nameof(Categories), new { transactionTypeFilter });
    }

    private IQueryable<SystemLogRowViewModel> BuildLogQuery()
    {
        return _db.SystemLogs
            .Select(x => new SystemLogRowViewModel
            {
                Level = x.Level,
                Action = x.Action,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                UserDisplayName = x.User != null ? x.User.DisplayName : null
            });
    }
}
