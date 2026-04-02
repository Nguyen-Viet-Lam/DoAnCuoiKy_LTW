using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Web.Models.Entities;

public class SystemLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required, MaxLength(16)]
    public string Level { get; set; } = "Info";

    [Required, MaxLength(64)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Data { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }
}

public class ReportDispatchLog
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(32)]
    public string ReportType { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string PeriodKey { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;
}
