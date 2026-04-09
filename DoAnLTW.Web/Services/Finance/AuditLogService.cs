using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;

namespace DoAnLTW.Web.Services.Finance;

/// <summary>Service ghi lại các hành động quan trọng vào bảng log để phục vụ kiểm soát và truy vết.</summary>
public class AuditLogService
{
    private readonly FinanceDbContext _db;

    /// <summary>
    /// Khởi tạo lớp AuditLogService và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public AuditLogService(FinanceDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Ghi một dòng nhật ký hệ thống phục vụ theo dõi và truy vết thao tác.
    /// </summary>
    public async Task WriteAsync(
        string action,
        string message,
        int? userId = null,
        string level = "Info",
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        _db.SystemLogs.Add(new SystemLog
        {
            UserId = userId,
            Action = action,
            Message = message,
            Level = level,
            Data = data,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
