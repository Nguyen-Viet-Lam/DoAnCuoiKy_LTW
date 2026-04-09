namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model biểu diễn một dòng log để hiển thị trong bảng log của admin.</summary>
public class SystemLogRowViewModel
{
    /// <summary>
    /// Mức độ log như Info, Warning hoặc Error.
    /// </summary>
    public string Level { get; set; } = string.Empty;
    /// <summary>
    /// Tên hành động hoặc sự kiện đã được ghi log.
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// Nội dung thông báo, cảnh báo hoặc log được hiển thị cho người đọc.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Tên hiển thị của người dùng gắn với dòng log này.
    /// </summary>
    public string? UserDisplayName { get; set; }
}
