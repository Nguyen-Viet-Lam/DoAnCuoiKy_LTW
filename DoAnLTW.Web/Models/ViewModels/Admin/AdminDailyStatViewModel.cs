namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model lưu thống kê theo ngày cho dashboard quản trị.</summary>
public class AdminDailyStatViewModel
{
    /// <summary>
    /// Nhãn hiển thị trên biểu đồ, bảng thống kê hoặc màn hình quản trị.
    /// </summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>
    /// Số lượng người dùng mới tạo trong ngày thống kê tương ứng.
    /// </summary>
    public int NewUsers { get; set; }
    /// <summary>
    /// Số lượng giao dịch phát sinh trong ngày thống kê tương ứng.
    /// </summary>
    public int Transactions { get; set; }
}
