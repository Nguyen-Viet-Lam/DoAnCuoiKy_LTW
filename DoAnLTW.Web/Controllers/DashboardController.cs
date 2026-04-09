using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

/// <summary>Controller tổng hợp dữ liệu dashboard, biểu đồ, cảnh báo và các chỉ số chi tiêu.</summary>
[Authorize(Roles = "User,Admin")]
public class DashboardController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly ForecastService _forecastService;
    private readonly WalletBalanceMonitorService _walletBalanceMonitorService;

    /// <summary>
    /// Khởi tạo lớp DashboardController và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public DashboardController(
        FinanceDbContext db,
        ForecastService forecastService,
        WalletBalanceMonitorService walletBalanceMonitorService)
    {
        _db = db;
        _forecastService = forecastService;
        _walletBalanceMonitorService = walletBalanceMonitorService;
    }

    /// <summary>
    /// Tổng hợp dữ liệu dashboard, biểu đồ, dự báo và cảnh báo của người dùng.
    /// </summary>
    public async Task<IActionResult> Index(int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        var userId = CurrentUserId;
        var now = DateTime.Today;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var monthRangeStart = currentMonthStart.AddMonths(-11);

        var wallets = await _db.Wallets
            .Where(x => x.UserId == userId && !x.IsArchived)
            .OrderByDescending(x => x.CurrentBalance)
            .ToListAsync(cancellationToken);

        var transactions = await _db.Transactions
            .Include(x => x.Category)
            .Include(x => x.Wallet)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.OccurredOn)
            .ToListAsync(cancellationToken);

        var monthlyComparison = Enumerable.Range(0, 12)
            .Select(offset => monthRangeStart.AddMonths(offset))
            .Select(month => new MonthlyComparisonItem
            {
                Label = month.ToString("MM/yyyy"),
                Month = month.Month,
                Year = month.Year,
                Income = transactions
                    .Where(x => x.Type == "Income" && x.OccurredOn >= month && x.OccurredOn < month.AddMonths(1))
                    .Sum(x => x.Amount),
                Expense = transactions
                    .Where(x => x.Type == "Expense" && x.OccurredOn >= month && x.OccurredOn < month.AddMonths(1))
                    .Sum(x => x.Amount)
            })
            .ToList();

        var selectedMonthItem = monthlyComparison
            .FirstOrDefault(x => x.Month == month && x.Year == year)
            ?? monthlyComparison.Last();

        var selectedMonthStart = new DateTime(selectedMonthItem.Year, selectedMonthItem.Month, 1);
        var selectedMonthEnd = selectedMonthStart.AddMonths(1);
        var monthTransactions = transactions
            .Where(x => x.OccurredOn >= selectedMonthStart && x.OccurredOn < selectedMonthEnd)
            .ToList();

        var categoryBreakdown = monthTransactions
            .Where(x => x.Type == "Expense")
            .GroupBy(x => new { x.Category.Name, x.Category.ColorHex })
            .Select(g => new CategoryTotalItem
            {
                CategoryName = g.Key.Name,
                Amount = g.Sum(x => x.Amount),
                ColorHex = g.Key.ColorHex
            })
            .OrderByDescending(x => x.Amount)
            .Take(5)
            .ToList();

        var sevenDayAnchor = selectedMonthItem.Year == currentMonthStart.Year && selectedMonthItem.Month == currentMonthStart.Month
            ? now
            : selectedMonthEnd.AddDays(-1);
        var sevenDayStart = sevenDayAnchor.AddDays(-6);
        if (sevenDayStart < selectedMonthStart)
        {
            sevenDayStart = selectedMonthStart;
        }

        var sevenDayExpenses = Enumerable.Range(0, 7)
            .Select(offset => sevenDayStart.AddDays(offset))
            .Where(day => day < selectedMonthEnd)
            .Select(day => new
            {
                Day = day,
                Amount = transactions
                    .Where(x => x.Type == "Expense" && x.OccurredOn.Date == day)
                    .Sum(x => x.Amount)
            })
            .ToList();

        var maxBar = sevenDayExpenses.Count == 0 ? 0m : sevenDayExpenses.Max(x => x.Amount);
        var budgetItems = await BuildBudgetProgressAsync(userId, selectedMonthStart, cancellationToken);
        var totalIncomeMonth = monthTransactions.Where(x => x.Type == "Income").Sum(x => x.Amount);
        var totalExpenseMonth = monthTransactions.Where(x => x.Type == "Expense").Sum(x => x.Amount);
        var forecast = selectedMonthItem.Year == currentMonthStart.Year && selectedMonthItem.Month == currentMonthStart.Month
            ? await _forecastService.CalculateMonthEndExpenseForecastAsync(userId, now, cancellationToken)
            : totalExpenseMonth;

        var model = new DashboardViewModel
        {
            DisplayName = User.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Ban",
            TotalBalance = wallets.Sum(x => x.CurrentBalance),
            LowBalanceThreshold = WalletBalanceMonitorService.LowBalanceThreshold,
            SelectedMonth = selectedMonthItem.Month,
            SelectedYear = selectedMonthItem.Year,
            SelectedMonthLabel = selectedMonthItem.FullLabel,
            IsCurrentMonthSelected = selectedMonthItem.Month == currentMonthStart.Month && selectedMonthItem.Year == currentMonthStart.Year,
            TotalIncomeMonth = totalIncomeMonth,
            TotalExpenseMonth = totalExpenseMonth,
            ForecastExpense = forecast,
            SavingsRate = totalIncomeMonth == 0 ? 0 : Math.Round((double)((totalIncomeMonth - totalExpenseMonth) / totalIncomeMonth * 100m), 1),
            UnreadAlertCount = await _db.BudgetAlerts.CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken),
            Wallets = wallets.Take(4).Select(x => new WalletMiniCardViewModel
            {
                Name = x.Name,
                Type = x.Type,
                Balance = x.CurrentBalance,
                IsLowBalance = _walletBalanceMonitorService.IsLowBalance(x.CurrentBalance)
            }).ToList(),
            SpendingSevenDays = sevenDayExpenses.Select(x => new ChartBarItem
            {
                Label = x.Day.ToString("dd/MM"),
                Amount = x.Amount,
                HeightPercent = maxBar == 0 ? 10 : (double)(x.Amount / maxBar * 100m)
            }).ToList(),
            CategoryBreakdown = categoryBreakdown,
            MonthlyComparison = monthlyComparison,
            BudgetHighlights = budgetItems,
            LatestAlerts = await _db.BudgetAlerts
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new AlertItemViewModel
                {
                    Id = x.Id,
                    Message = x.Message,
                    UsagePercent = x.UsagePercent,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken),
            RecentTransactions = transactions
                .Take(6)
                .Select(x => new TransactionHistoryItemViewModel
                {
                    Id = x.Id,
                    Date = x.OccurredOn,
                    WalletName = x.Wallet.Name,
                    CategoryName = x.Category.Name,
                    CategoryColor = x.Category.ColorHex,
                    CategoryIcon = x.Category.Icon,
                    Type = x.Type,
                    Amount = x.Amount,
                    Note = x.Note,
                    UsedAiSuggestion = x.AiSuggestedCategoryId == x.CategoryId
                })
                .ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Đánh dấu một cảnh báo ngân sách là đã đọc.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAlertAsRead(int id, CancellationToken cancellationToken)
    {
        var alert = await _db.BudgetAlerts
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId, cancellationToken);

        if (alert is not null && !alert.IsRead)
        {
            alert.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Đánh dấu toàn bộ cảnh báo ngân sách của người dùng là đã đọc.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAlertsAsRead(CancellationToken cancellationToken)
    {
        var alerts = await _db.BudgetAlerts
            .Where(x => x.UserId == CurrentUserId && !x.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var alert in alerts)
        {
            alert.IsRead = true;
        }

        if (alerts.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<BudgetProgressViewModel>> BuildBudgetProgressAsync(int userId, DateTime monthStart, CancellationToken cancellationToken)
    {
        var budgets = await _db.Budgets
            .Include(x => x.Category)
            .Where(x => x.UserId == userId && x.Year == monthStart.Year && x.Month == monthStart.Month)
            .ToListAsync(cancellationToken);

        var items = new List<BudgetProgressViewModel>();
        foreach (var budget in budgets)
        {
            var spent = await _db.Transactions
                .Where(x => x.UserId == userId &&
                            x.CategoryId == budget.CategoryId &&
                            x.Type == "Expense" &&
                            x.OccurredOn.Year == budget.Year &&
                            x.OccurredOn.Month == budget.Month)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            items.Add(new BudgetProgressViewModel
            {
                Id = budget.Id,
                CategoryName = budget.Category.Name,
                ColorHex = budget.Category.ColorHex,
                LimitAmount = budget.LimitAmount,
                SpentAmount = spent,
                UsagePercent = budget.LimitAmount == 0 ? 0 : (double)(spent / budget.LimitAmount * 100m),
                AlertThresholdPercent = budget.AlertThresholdPercent,
                MonthLabel = $"{budget.Month:00}/{budget.Year}"
            });
        }

        return items.OrderByDescending(x => x.UsagePercent).Take(4).ToList();
    }
}
