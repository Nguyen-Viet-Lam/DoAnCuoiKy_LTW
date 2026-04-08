using DoAnLTW.Web.Data;
using DoAnLTW.Web.Hubs;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Services.Email;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

public sealed record BudgetAlertCheckResult(
    bool AlertCreated,
    bool IsExceeded,
    string Message,
    decimal SpentAmount,
    decimal LimitAmount,
    double UsagePercent)
{
    public static BudgetAlertCheckResult None { get; } = new(
        false,
        false,
        string.Empty,
        0m,
        0m,
        0d);
}

public class BudgetMonitorService
{
    private readonly FinanceDbContext _db;
    private readonly IHubContext<BudgetHub> _hubContext;
    private readonly EmailQueue _emailQueue;

    public BudgetMonitorService(FinanceDbContext db, IHubContext<BudgetHub> hubContext, EmailQueue emailQueue)
    {
        _db = db;
        _hubContext = hubContext;
        _emailQueue = emailQueue;
    }

    public async Task<BudgetAlertCheckResult> CheckAndNotifyAsync(
        int userId,
        int categoryId,
        int transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _db.Transactions
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId, cancellationToken);

        if (transaction is null || transaction.Type != "Expense")
        {
            return BudgetAlertCheckResult.None;
        }

        var year = transaction.OccurredOn.Year;
        var month = transaction.OccurredOn.Month;

        var budget = await _db.Budgets
            .Include(x => x.Category)
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.CategoryId == categoryId &&
                     x.Year == year &&
                     x.Month == month,
                cancellationToken);

        if (budget is null)
        {
            return BudgetAlertCheckResult.None;
        }

        var spentAmount = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.CategoryId == categoryId &&
                        x.Type == "Expense" &&
                        x.OccurredOn.Year == year &&
                        x.OccurredOn.Month == month)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var usagePercent = budget.LimitAmount == 0
            ? 0
            : (double)(spentAmount / budget.LimitAmount * 100m);

        if (usagePercent < 100)
        {
            return BudgetAlertCheckResult.None;
        }

        var alertExists = await _db.BudgetAlerts
            .AnyAsync(x => x.BudgetId == budget.Id && x.WalletTransactionId == transactionId, cancellationToken);

        if (alertExists)
        {
            return BudgetAlertCheckResult.None;
        }

        var message = $"Ban da vuot ngan sach {budget.Category.Name} thang {month}/{year}.";
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

        await TrySendBudgetAlertEmailAsync(
            userId,
            budget,
            alert,
            spentAmount,
            usagePercent,
            year,
            month,
            cancellationToken);

        return new BudgetAlertCheckResult(
            true,
            true,
            message,
            spentAmount,
            budget.LimitAmount,
            usagePercent);
    }

    public async Task<BudgetAlertCheckResult> CheckBudgetStatusAsync(
        int userId,
        int categoryId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var latestTransactionId = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.CategoryId == categoryId &&
                        x.Type == "Expense" &&
                        x.OccurredOn.Year == year &&
                        x.OccurredOn.Month == month)
            .OrderByDescending(x => x.OccurredOn)
            .ThenByDescending(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!latestTransactionId.HasValue)
        {
            return BudgetAlertCheckResult.None;
        }

        return await CheckAndNotifyAsync(userId, categoryId, latestTransactionId.Value, cancellationToken);
    }

    private async Task TrySendBudgetAlertEmailAsync(
        int userId,
        Budget budget,
        BudgetAlert currentAlert,
        decimal spentAmount,
        double usagePercent,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var alreadySent = await _db.BudgetAlerts
            .AnyAsync(x => x.BudgetId == budget.Id && x.IsEmailSent && x.Id != currentAlert.Id, cancellationToken);

        if (alreadySent)
        {
            return;
        }

        var userEmail = await _db.Users
            .Where(x => x.Id == userId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return;
        }

        await _emailQueue.QueueAsync(new EmailMessage
        {
            To = userEmail,
            Subject = $"Vuot ngan sach {budget.Category.Name} thang {month}/{year}",
            HtmlBody = BuildBudgetAlertEmailHtml(
                budget.Category.Name,
                spentAmount,
                budget.LimitAmount,
                usagePercent,
                month,
                year)
        });

        currentAlert.IsEmailSent = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string BuildBudgetAlertEmailHtml(
        string categoryName,
        decimal spentAmount,
        decimal limitAmount,
        double usagePercent,
        int month,
        int year)
    {
        var overAmount = Math.Max(0m, spentAmount - limitAmount);
        var progressBarWidth = Math.Min(usagePercent, 100d);

        return $$"""
                 <!DOCTYPE html>
                 <html lang="vi">
                 <head>
                     <meta charset="utf-8" />
                     <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                     <title>Vuot ngan sach</title>
                 </head>
                 <body style="margin:0;padding:24px;background:#f6f9fc;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;">
                     <div style="max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,0.08);">
                         <div style="padding:28px 24px;background:linear-gradient(135deg,#ef4444,#dc2626);color:#ffffff;">
                             <div style="font-size:28px;font-weight:700;">Vuot ngan sach</div>
                             <p style="margin:10px 0 0;line-height:1.6;">
                                 Danh muc <strong>{{categoryName}}</strong> da vuot ngan sach thang {{month}}/{{year}}.
                             </p>
                         </div>
                         <div style="padding:24px;">
                             <div style="display:grid;gap:12px;">
                                 <div style="padding:16px;border-radius:14px;background:#f8fafc;border:1px solid #e5e7eb;">
                                     <div style="display:flex;justify-content:space-between;gap:12px;">
                                         <span>Da chi</span>
                                         <strong>{{spentAmount:N0}} VND</strong>
                                     </div>
                                 </div>
                                 <div style="padding:16px;border-radius:14px;background:#f8fafc;border:1px solid #e5e7eb;">
                                     <div style="display:flex;justify-content:space-between;gap:12px;">
                                         <span>Ngan sach</span>
                                         <strong>{{limitAmount:N0}} VND</strong>
                                     </div>
                                 </div>
                                 <div style="padding:16px;border-radius:14px;background:#fff1f2;border:1px solid #fecdd3;">
                                     <div style="display:flex;justify-content:space-between;gap:12px;">
                                         <span>Vuot</span>
                                         <strong style="color:#dc2626;">{{overAmount:N0}} VND</strong>
                                     </div>
                                 </div>
                             </div>
                             <div style="margin-top:20px;">
                                 <div style="display:flex;justify-content:space-between;gap:12px;font-size:14px;color:#6b7280;">
                                     <span>Muc su dung</span>
                                     <strong style="color:#dc2626;">{{usagePercent:0.#}}%</strong>
                                 </div>
                                 <div style="margin-top:8px;height:12px;background:#e5e7eb;border-radius:999px;overflow:hidden;">
                                     <div style="width:{{progressBarWidth:0.#}}%;height:100%;background:linear-gradient(135deg,#ef4444,#dc2626);"></div>
                                 </div>
                             </div>
                             <p style="margin:20px 0 0;line-height:1.7;color:#4b5563;">
                                 He thong gui email nay de ban biet rang chi tieu da vuot ngan sach va can dieu chinh som.
                             </p>
                         </div>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
