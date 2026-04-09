namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp kết quả báo cáo gồm thu, chi, chênh lệch và danh sách giao dịch.</summary>
public class ReportSummaryViewModel
{
    /// <summary>
    /// Mốc bắt đầu của kỳ báo cáo đã tổng hợp.
    /// </summary>
    public DateTime From { get; set; }
    /// <summary>
    /// Mốc kết thúc của kỳ báo cáo đã tổng hợp.
    /// </summary>
    public DateTime To { get; set; }
    /// <summary>
    /// Tổng thu trong kỳ báo cáo hoặc khoảng thời gian đang xét.
    /// </summary>
    public decimal TotalIncome { get; set; }
    /// <summary>
    /// Tổng chi trong kỳ báo cáo hoặc khoảng thời gian đang xét.
    /// </summary>
    public decimal TotalExpense { get; set; }
    /// <summary>
    /// Chênh lệch giữa tổng thu và tổng chi trong kỳ.
    /// </summary>
    public decimal BalanceChange { get; set; }
    /// <summary>
    /// Tổng số giao dịch liên quan đến bản ghi hoặc thống kê hiện tại.
    /// </summary>
    public int TransactionCount { get; set; }
    /// <summary>
    /// Mức chi trung bình mỗi ngày trong khoảng báo cáo đang xét.
    /// </summary>
    public decimal AverageExpensePerDay { get; set; }
    /// <summary>
    /// Danh sách danh mục chi tiêu lớn nhất trong kỳ báo cáo.
    /// </summary>
    public List<CategoryTotalItem> TopCategories { get; set; } = [];
    /// <summary>
    /// Danh sách giao dịch liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public List<ReportTransactionItem> Transactions { get; set; } = [];
}
