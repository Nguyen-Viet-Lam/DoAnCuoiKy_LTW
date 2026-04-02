using DoAnLTW.Web.Data;
using DoAnLTW.Web.Hubs;
using DoAnLTW.Web.Models.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

public class BudgetMonitorService
{
    private readonly FinanceDbContext _db;
    private readonly IHubContext<BudgetHub> _hubContext;

    public BudgetMonitorService(FinanceDbContext db, IHubContext<BudgetHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task CheckAndNotifyAsync(int userId, int categoryId, int transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _db.Transactions
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId, cancellationToken);

        if (transaction is null || transaction.Type != "Expense")
        {
            return;
        }

        var year = transaction.OccurredOn.Year;
        var month = transaction.OccurredOn.Month;

        var budget = await _db.Budgets
            .Include(x => x.Category)
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.CategoryId == categoryId && x.Year == year && x.Month == month,
                cancellationToken);

        if (budget is null)
        {
            return;
        }

        var spentAmount = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.CategoryId == categoryId &&
                        x.Type == "Expense" &&
                        x.OccurredOn.Year == year &&
                        x.OccurredOn.Month == month)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var usagePercent = budget.LimitAmount == 0 ? 0 : (double)(spentAmount / budget.LimitAmount * 100m);
        if (usagePercent < budget.AlertThresholdPercent)
        {
            return;
        }

        var alertExists = await _db.BudgetAlerts
            .AnyAsync(x => x.BudgetId == budget.Id && x.WalletTransactionId == transactionId, cancellationToken);

        if (alertExists)
        {
            return;
        }

        var message = usagePercent >= 100
            ? $"Bạn đã vượt ngân sách {budget.Category.Name} tháng {month}/{year}."
            : $"Ngân sách {budget.Category.Name} đã dùng {usagePercent:0.#}% tháng {month}/{year}.";

        var alert = new BudgetAlert
        {
            UserId = userId,
            BudgetId = budget.Id,
            WalletTransactionId = transactionId,
            Message = message,
            SpentAmount = spentAmount,
            LimitAmount = budget.LimitAmount,
            UsagePercent = usagePercent,
            CreatedAt = DateTime.UtcNow
        };

        _db.BudgetAlerts.Add(alert);
        await _db.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.Group($"user-{userId}")
            .SendAsync("budgetAlert", new
            {
                message,
                usagePercent = Math.Round(usagePercent, 1),
                spentAmount,
                limitAmount = budget.LimitAmount,
                category = budget.Category.Name
            }, cancellationToken);
    }
}
