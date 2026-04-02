using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;

namespace DoAnLTW.Web.Services.Finance;

public class AuditLogService
{
    private readonly FinanceDbContext _db;

    public AuditLogService(FinanceDbContext db)
    {
        _db = db;
    }

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
