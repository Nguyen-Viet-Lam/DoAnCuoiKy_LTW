using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

/// <summary>Background service tự động kiểm tra lịch và đưa báo cáo tuần của người dùng vào hàng đợi email.</summary>
public class WeeklyReportHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WeeklyReportHostedService> _logger;

    /// <summary>
    /// Khởi tạo lớp WeeklyReportHostedService và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public WeeklyReportHostedService(IServiceProvider serviceProvider, ILogger<WeeklyReportHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendWeeklyReportsIfNeededAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi báo cáo tuần tự động.");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task SendWeeklyReportsIfNeededAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        if (now.DayOfWeek != DayOfWeek.Monday || now.Hour < 8 || now.Hour > 9)
        {
            return;
        }

        var weekStart = now.Date.AddDays(-7);
        var weekEnd = now.Date.AddDays(-1);
        var periodKey = $"{weekStart:yyyyMMdd}-{weekEnd:yyyyMMdd}";

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var reportService = scope.ServiceProvider.GetRequiredService<ReportService>();
        var emailQueue = scope.ServiceProvider.GetRequiredService<EmailQueue>();

        var users = await db.Users
            .Include(x => x.Role)
            .Where(x => x.IsActive && x.IsEmailVerified && x.Role.Name == "User")
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var alreadySent = await db.ReportDispatchLogs
                .AnyAsync(x => x.UserId == user.Id && x.ReportType == "Weekly" && x.PeriodKey == periodKey, cancellationToken);

            if (alreadySent)
            {
                continue;
            }

            var emailBody = await reportService.BuildWeeklyEmailAsync(user.Id, weekStart, weekEnd, cancellationToken);
            await emailQueue.QueueAsync(new EmailMessage
            {
                To = user.Email,
                Subject = $"Tóm tắt chi tiêu tuần {weekStart:dd/MM} - {weekEnd:dd/MM}",
                HtmlBody = emailBody
            });

            db.ReportDispatchLogs.Add(new ReportDispatchLog
            {
                UserId = user.Id,
                ReportType = "Weekly",
                PeriodKey = periodKey,
                SentAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
