namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model lưu dữ liệu so sánh thu và chi của một tháng trên dashboard.</summary>
public class MonthlyComparisonItem
{
    /// <summary>
    /// Nhãn hiển thị trên biểu đồ, bảng thống kê hoặc màn hình quản trị.
    /// </summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>
    /// Tháng áp dụng của ngân sách, thống kê hoặc khoảng dữ liệu.
    /// </summary>
    public int Month { get; set; }
    /// <summary>
    /// Năm áp dụng của ngân sách, thống kê hoặc khoảng dữ liệu.
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// Tổng thu của tháng tương ứng trong biểu đồ so sánh.
    /// </summary>
    public decimal Income { get; set; }
    /// <summary>
    /// Tổng chi của tháng tương ứng trong biểu đồ so sánh.
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// Nhãn rút gọn của tháng để hiển thị gọn trên giao diện hoặc biểu đồ.
    /// </summary>
    public string ShortLabel => $"T{Month}";
    /// <summary>
    /// Nhãn đầy đủ của tháng và năm đang được biểu diễn.
    /// </summary>
    public string FullLabel => $"Tháng {Month}/{Year}";
    /// <summary>
    /// Chênh lệch giữa tổng thu và tổng chi của tháng này.
    /// </summary>
    public decimal NetAmount => Income - Expense;
}
