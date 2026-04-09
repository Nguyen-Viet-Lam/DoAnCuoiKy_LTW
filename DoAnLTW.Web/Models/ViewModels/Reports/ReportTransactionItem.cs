namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model biểu diễn một giao dịch trong kết quả báo cáo.</summary>
public class ReportTransactionItem
{
    /// <summary>
    /// Ngày của giao dịch hoặc mốc thời gian cần hiển thị.
    /// </summary>
    public DateTime Date { get; set; }
    /// <summary>
    /// Tên ví được hiển thị cùng giao dịch hoặc báo cáo.
    /// </summary>
    public string WalletName { get; set; } = string.Empty;
    /// <summary>
    /// Tên danh mục được hiển thị cùng giao dịch hoặc thống kê.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    /// <summary>
    /// Loại giao dịch, thường là Income hoặc Expense.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Số tiền của giao dịch, thống kê hoặc mục dữ liệu tương ứng.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Ghi chú mô tả nội dung giao dịch.
    /// </summary>
    public string Note { get; set; } = string.Empty;
}
