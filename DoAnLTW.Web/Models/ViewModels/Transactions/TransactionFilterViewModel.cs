namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model lưu bộ lọc tìm kiếm và phân loại danh sách giao dịch.</summary>
public class TransactionFilterViewModel
{
    /// <summary>
    /// Loại giao dịch, thường là Income hoặc Expense.
    /// </summary>
    public string Type { get; set; } = "Expense";
    /// <summary>
    /// Mã ví được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    public int? WalletId { get; set; }
    /// <summary>
    /// Mã danh mục được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    public int? CategoryId { get; set; }
    /// <summary>
    /// Từ khóa mà hệ thống học được từ ghi chú để gợi ý danh mục.
    /// </summary>
    public string Keyword { get; set; } = string.Empty;
    /// <summary>
    /// Ngày bắt đầu của khoảng lọc hoặc kỳ báo cáo.
    /// </summary>
    public DateTime? FromDate { get; set; }
    /// <summary>
    /// Ngày kết thúc của khoảng lọc hoặc kỳ báo cáo.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Danh sách ví dùng để đổ vào ô chọn trên form.
    /// </summary>
    public List<SelectOptionViewModel> WalletOptions { get; set; } = [];
    /// <summary>
    /// Danh sách danh mục dùng để đổ vào ô chọn trên form.
    /// </summary>
    public List<SelectOptionViewModel> CategoryOptions { get; set; } = [];
}
