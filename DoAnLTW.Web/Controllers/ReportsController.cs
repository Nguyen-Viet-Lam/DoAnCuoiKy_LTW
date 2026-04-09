using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Email;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

/// <summary>Controller xem báo cáo, xuất Excel và gửi báo cáo qua email.</summary>
[Authorize(Roles = "User")]
public class ReportsController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly ReportService _reportService;
    private readonly EmailQueue _emailQueue;

    /// <summary>
    /// Khởi tạo lớp ReportsController và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public ReportsController(FinanceDbContext db, ReportService reportService, EmailQueue emailQueue)
    {
        _db = db;
        _reportService = reportService;
        _emailQueue = emailQueue;
    }

    /// <summary>
    /// Tổng hợp báo cáo tuần, tháng và khoảng ngày tùy chọn để hiển thị lên giao diện.
    /// </summary>
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var weekStart = today.AddDays(-6);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var customFrom = fromDate?.Date ?? monthStart;
        var customTo = toDate?.Date ?? today;

        if (customFrom > customTo)
        {
            TempData["ErrorMessage"] = "Khoang thoi gian loc bao cao chua hop le.";
            customFrom = monthStart;
            customTo = today;
        }

        var defaultEmail = await _db.Users
            .Where(x => x.Id == CurrentUserId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var model = new ReportsPageViewModel
        {
            Filter = new ReportFilterViewModel
            {
                FromDate = customFrom,
                ToDate = customTo
            },
            WeeklySummary = await _reportService.BuildSummaryAsync(CurrentUserId, weekStart, today, cancellationToken),
            MonthlySummary = await _reportService.BuildSummaryAsync(CurrentUserId, monthStart, today, cancellationToken),
            CustomSummary = await _reportService.BuildSummaryAsync(CurrentUserId, customFrom, customTo, cancellationToken),
            DefaultEmail = defaultEmail
        };

        return View(model);
    }

    /// <summary>
    /// Xuất dữ liệu báo cáo theo kỳ được chọn thành tệp Excel để tải về.
    /// </summary>
    public async Task<IActionResult> ExportExcel(
        string period = "month",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var from = period switch
        {
            "week" => today.AddDays(-6),
            "custom" when fromDate.HasValue && toDate.HasValue => fromDate.Value.Date,
            _ => new DateTime(today.Year, today.Month, 1)
        };
        var to = period == "custom" && toDate.HasValue ? toDate.Value.Date : today;

        if (from > to)
        {
            TempData["ErrorMessage"] = "Khoang thoi gian xuat bao cao chua hop le.";
            return RedirectToAction(nameof(Index), new { fromDate, toDate });
        }

        var bytes = await _reportService.ExportExcelAsync(CurrentUserId, from, to, cancellationToken);
        var fileName = period switch
        {
            "week" => $"bao-cao-7-ngay-{today:yyyyMMdd}.xlsx",
            "custom" => $"bao-cao-{from:yyyyMMdd}-{to:yyyyMMdd}.xlsx",
            _ => $"bao-cao-thang-{today:yyyyMM}.xlsx"
        };

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Đưa email báo cáo tuần vào hàng đợi gửi nền và ghi log lịch sử gửi.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendWeeklyToEmail(string email, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var body = await _reportService.BuildWeeklyEmailAsync(CurrentUserId, today.AddDays(-6), today, cancellationToken);

        await _emailQueue.QueueAsync(new EmailMessage
        {
            To = email,
            Subject = $"Bao cao chi tieu tuan {today.AddDays(-6):dd/MM} - {today:dd/MM}",
            HtmlBody = body
        });

        _db.ReportDispatchLogs.Add(new ReportDispatchLog
        {
            UserId = CurrentUserId,
            ReportType = "ManualWeekly",
            PeriodKey = $"{today.AddDays(-6):yyyyMMdd}-{today:yyyyMMdd}-{DateTime.UtcNow:HHmmssfff}",
            SentAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Da dua email bao cao tuan vao hang doi gui.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Đưa email báo cáo theo khoảng ngày tùy chọn vào hàng đợi gửi nền.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSummaryToEmail(string email, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        if (from > to)
        {
            TempData["ErrorMessage"] = "Khoang thoi gian gui bao cao chua hop le.";
            return RedirectToAction(nameof(Index), new { fromDate, toDate });
        }

        var body = await _reportService.BuildWeeklyEmailAsync(CurrentUserId, from, to, cancellationToken);

        await _emailQueue.QueueAsync(new EmailMessage
        {
            To = email,
            Subject = $"Bao cao chi tieu {from:dd/MM} - {to:dd/MM}",
            HtmlBody = body
        });

        _db.ReportDispatchLogs.Add(new ReportDispatchLog
        {
            UserId = CurrentUserId,
            ReportType = "ManualCustom",
            PeriodKey = $"{from:yyyyMMdd}-{to:yyyyMMdd}-{DateTime.UtcNow:HHmmssfff}",
            SentAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Da dua email bao cao theo khoang ngay vao hang doi gui.";
        return RedirectToAction(nameof(Index), new { fromDate, toDate });
    }
}
