using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Web.Models.Entities;

/// <summary>Entity lưu nhật ký hoạt động và sự kiện quan trọng của hệ thống.</summary>
public class SystemLog
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Mức độ log như Info, Warning hoặc Error.
    /// </summary>
    [Required, MaxLength(16)]
    public string Level { get; set; } = "Info";

    /// <summary>
    /// Tên hành động hoặc sự kiện đã được ghi log.
    /// </summary>
    [Required, MaxLength(64)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung thông báo, cảnh báo hoặc log được hiển thị cho người đọc.
    /// </summary>
    [Required, MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu phụ bổ sung đi kèm log để phục vụ tra cứu.
    /// </summary>
    [MaxLength(2000)]
    public string? Data { get; set; }

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }
}

/// <summary>Entity lưu lịch sử các lần gửi báo cáo qua email để tránh gửi trùng.</summary>
public class ReportDispatchLog
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Loại báo cáo đã được gửi hoặc đang được xử lý.
    /// </summary>
    [Required, MaxLength(32)]
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Khóa kỳ báo cáo dùng để nhận diện duy nhất mỗi lần gửi.
    /// </summary>
    [Required, MaxLength(32)]
    public string PeriodKey { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm email báo cáo được gửi hoặc được ghi nhận là đã gửi.
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;
}
