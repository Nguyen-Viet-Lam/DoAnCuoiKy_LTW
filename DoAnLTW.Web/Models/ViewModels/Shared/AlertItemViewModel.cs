namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model biểu diễn một cảnh báo ngân sách để hiển thị trên giao diện.</summary>
public class AlertItemViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Nội dung thông báo, cảnh báo hoặc log được hiển thị cho người đọc.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Tỷ lệ sử dụng ngân sách hoặc mức độ hoàn thành, tính theo phần trăm.
    /// </summary>
    public double UsagePercent { get; set; }
    /// <summary>
    /// Cho biết cảnh báo đã được người dùng đọc hay chưa.
    /// </summary>
    public bool IsRead { get; set; }
    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
