namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model biểu diễn một cột dữ liệu trong biểu đồ dạng bar.</summary>
public class ChartBarItem
{
    /// <summary>
    /// Nhãn hiển thị trên biểu đồ, bảng thống kê hoặc màn hình quản trị.
    /// </summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>
    /// Số tiền của giao dịch, thống kê hoặc mục dữ liệu tương ứng.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Chiều cao cột theo phần trăm để tiện render tương đối trên biểu đồ.
    /// </summary>
    public double HeightPercent { get; set; }
}
