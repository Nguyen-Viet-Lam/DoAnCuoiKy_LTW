namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model biểu diễn một dòng giao dịch dùng cho bảng lịch sử và timeline.</summary>
public class TransactionHistoryItemViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int Id { get; set; }
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
    /// Màu của danh mục dùng để tô giao diện hoặc biểu đồ.
    /// </summary>
    public string CategoryColor { get; set; } = "#8ecae6";
    /// <summary>
    /// Biểu tượng của danh mục dùng để minh họa giao dịch.
    /// </summary>
    public string CategoryIcon { get; set; } = "bi-circle";
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
    /// <summary>
    /// Cho biết danh mục cuối cùng có trùng với gợi ý tự động hay không.
    /// </summary>
    public bool UsedAiSuggestion { get; set; }
}
