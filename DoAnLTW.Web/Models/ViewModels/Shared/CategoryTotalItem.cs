namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model lưu tổng số tiền theo một danh mục trong dashboard hoặc báo cáo.</summary>
public class CategoryTotalItem
{
    /// <summary>
    /// Tên danh mục được hiển thị cùng giao dịch hoặc thống kê.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    /// <summary>
    /// Số tiền của giao dịch, thống kê hoặc mục dữ liệu tương ứng.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Mã màu hiển thị dùng cho danh mục, thẻ hoặc biểu đồ.
    /// </summary>
    public string ColorHex { get; set; } = "#8ecae6";
}
