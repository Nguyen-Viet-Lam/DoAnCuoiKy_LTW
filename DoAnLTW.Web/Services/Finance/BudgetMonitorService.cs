using DoAnLTW.Web.Data;
using DoAnLTW.Web.Hubs;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Services.Email;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

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

        var isExceeded = usagePercent >= 100;
        var message = isExceeded
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

        await TrySendBudgetAlertEmailAsync(userId, budget, alert, isExceeded, spentAmount, usagePercent, year, month, cancellationToken);
    }

    private async Task TrySendBudgetAlertEmailAsync(
        int userId,
        Budget budget,
        BudgetAlert currentAlert,
        bool isExceeded,
        decimal spentAmount,
        double usagePercent,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        bool alreadySentForLevel;

        if (isExceeded)
        {
            alreadySentForLevel = await _db.BudgetAlerts
                .AnyAsync(x => x.BudgetId == budget.Id
                            && x.IsEmailSent
                            && x.UsagePercent >= 100
                            && x.Id != currentAlert.Id, cancellationToken);
        }
        else
        {
            alreadySentForLevel = await _db.BudgetAlerts
                .AnyAsync(x => x.BudgetId == budget.Id
                            && x.IsEmailSent
                            && x.UsagePercent < 100
                            && x.Id != currentAlert.Id, cancellationToken);
        }

        if (alreadySentForLevel)
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

        var subject = isExceeded
            ? $"🚨 Vượt ngân sách {budget.Category.Name} tháng {month}/{year}"
            : $"⚠️ Ngân sách {budget.Category.Name} đã dùng {usagePercent:0.#}% tháng {month}/{year}";

        var htmlBody = BuildBudgetAlertEmailHtml(
            budget.Category.Name,
            budget.Category.Icon,
            spentAmount,
            budget.LimitAmount,
            usagePercent,
            budget.AlertThresholdPercent,
            month,
            year,
            isExceeded);

        await _emailQueue.QueueAsync(new EmailMessage
        {
            To = userEmail,
            Subject = subject,
            HtmlBody = htmlBody
        });

        currentAlert.IsEmailSent = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string BuildBudgetAlertEmailHtml(
        string categoryName,
        string categoryIcon,
        decimal spentAmount,
        decimal limitAmount,
        double usagePercent,
        int thresholdPercent,
        int month,
        int year,
        bool isExceeded)
    {
        var headerColor = isExceeded ? "#dc3545" : "#fd7e14";
        var headerGradient = isExceeded
            ? "linear-gradient(135deg, #dc3545 0%, #c82333 100%)"
            : "linear-gradient(135deg, #fd7e14 0%, #e8590c 100%)";
        var statusText = isExceeded ? "ĐÃ VƯỢT NGÂN SÁCH" : "SẮP VƯỢT NGÂN SÁCH";
        var statusEmoji = isExceeded ? "🚨" : "⚠️";
        var progressBarColor = isExceeded ? "#dc3545" : "#fd7e14";
        var progressBarWidth = Math.Min(usagePercent, 100);

        var overAmount = spentAmount - limitAmount;
        var overSection = isExceeded
            ? $@"<tr>
                    <td style=""padding:12px 16px;color:#6c757d;font-size:14px;border-bottom:1px solid #f1f3f5;"">Vượt quá</td>
                    <td style=""padding:12px 16px;font-weight:700;color:#dc3545;text-align:right;font-size:14px;border-bottom:1px solid #f1f3f5;"">{overAmount:N0} VND</td>
                </tr>"
            : "";

        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{statusText}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f0f4f8;font-family:'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;"">
    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;background-color:#f0f4f8;padding:32px 16px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px;width:100%;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">
                    <tr>
                        <td style=""background:{headerGradient};padding:32px 24px;text-align:center;"">
                            <div style=""font-size:40px;margin-bottom:8px;"">{statusEmoji}</div>
                            <h1 style=""margin:0;color:#ffffff;font-size:20px;font-weight:700;letter-spacing:0.5px;"">{statusText}</h1>
                            <p style=""margin:8px 0 0;color:rgba(255,255,255,0.9);font-size:14px;"">
                                Danh mục: <strong>{categoryName}</strong> &mdash; Tháng {month}/{year}
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:28px 24px 0;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;"">
                                <tr>
                                    <td style=""font-size:13px;color:#6c757d;"">Đã sử dụng</td>
                                    <td style=""font-size:13px;color:{headerColor};font-weight:700;text-align:right;"">{usagePercent:0.#}%</td>
                                </tr>
                            </table>
                            <div style=""margin-top:8px;background:#e9ecef;border-radius:8px;height:12px;overflow:hidden;"">
                                <div style=""width:{progressBarWidth:0.#}%;height:100%;background:{progressBarColor};border-radius:8px;transition:width 0.3s;""></div>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:24px;"">
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;border:1px solid #e9ecef;border-radius:12px;overflow:hidden;"">
                                <tr style=""background:#f8f9fa;"">
                                    <td style=""padding:12px 16px;color:#6c757d;font-size:14px;border-bottom:1px solid #f1f3f5;"">Đã chi</td>
                                    <td style=""padding:12px 16px;font-weight:700;color:#212529;text-align:right;font-size:14px;border-bottom:1px solid #f1f3f5;"">{spentAmount:N0} VND</td>
                                </tr>
                                <tr>
                                    <td style=""padding:12px 16px;color:#6c757d;font-size:14px;border-bottom:1px solid #f1f3f5;"">Hạn mức</td>
                                    <td style=""padding:12px 16px;font-weight:700;color:#212529;text-align:right;font-size:14px;border-bottom:1px solid #f1f3f5;"">{limitAmount:N0} VND</td>
                                </tr>
                                <tr style=""background:#f8f9fa;"">
                                    <td style=""padding:12px 16px;color:#6c757d;font-size:14px;border-bottom:1px solid #f1f3f5;"">Ngưỡng cảnh báo</td>
                                    <td style=""padding:12px 16px;font-weight:700;color:{headerColor};text-align:right;font-size:14px;border-bottom:1px solid #f1f3f5;"">{thresholdPercent}%</td>
                                </tr>
                                {overSection}
                                <tr>
                                    <td style=""padding:12px 16px;color:#6c757d;font-size:14px;"">Danh mục</td>
                                    <td style=""padding:12px 16px;font-weight:700;color:#212529;text-align:right;font-size:14px;"">{categoryName}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:0 24px 24px;"">
                            <div style=""background:{(isExceeded ? "#fff5f5" : "#fff8e1")};border-left:4px solid {headerColor};border-radius:8px;padding:16px;"">
                                <p style=""margin:0;font-size:14px;color:#495057;line-height:1.6;"">
                                    {(isExceeded
                                        ? $"Bạn đã chi <strong>{spentAmount:N0} VND</strong>, vượt hạn mức <strong>{limitAmount:N0} VND</strong> cho danh mục <strong>{categoryName}</strong>. Hãy cân nhắc điều chỉnh chi tiêu để tránh thâm hụt ngân sách."
                                        : $"Chi tiêu danh mục <strong>{categoryName}</strong> đã đạt <strong>{usagePercent:0.#}%</strong> hạn mức tháng. Hãy theo dõi sát để không vượt ngân sách.")}
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background:#f8f9fa;padding:20px 24px;text-align:center;border-top:1px solid #e9ecef;"">
                            <p style=""margin:0;font-size:12px;color:#adb5bd;line-height:1.6;"">
                                Email này được gửi tự động từ <strong>Finance Flow</strong>.<br/>
                                Bạn nhận email này vì đã đặt ngưỡng cảnh báo ngân sách.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
