namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp toàn bộ dữ liệu cần hiển thị trên dashboard người dùng.</summary>
public class DashboardViewModel
{
    /// <summary>
    /// Tên hiển thị của người dùng trên giao diện.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>
    /// Tổng số dư cộng dồn của các ví đang được hiển thị.
    /// </summary>
    public decimal TotalBalance { get; set; }
    /// <summary>
    /// Ngưỡng số dư thấp dùng để tô cảnh báo trên giao diện.
    /// </summary>
    public decimal LowBalanceThreshold { get; set; }
    /// <summary>
    /// Tháng hiện đang được chọn để xem dashboard.
    /// </summary>
    public int SelectedMonth { get; set; }
    /// <summary>
    /// Năm hiện đang được chọn để xem dashboard.
    /// </summary>
    public int SelectedYear { get; set; }
    /// <summary>
    /// Nhãn hiển thị của tháng và năm đang được chọn.
    /// </summary>
    public string SelectedMonthLabel { get; set; } = string.Empty;
    /// <summary>
    /// Cho biết dashboard đang xem đúng tháng hiện tại hay không.
    /// </summary>
    public bool IsCurrentMonthSelected { get; set; }
    /// <summary>
    /// Tổng thu của tháng đang được chọn trên giao diện.
    /// </summary>
    public decimal TotalIncomeMonth { get; set; }
    /// <summary>
    /// Tổng chi của tháng đang được chọn trên giao diện.
    /// </summary>
    public decimal TotalExpenseMonth { get; set; }
    /// <summary>
    /// Giá trị dự báo tổng chi đến cuối kỳ hoặc cuối tháng.
    /// </summary>
    public decimal ForecastExpense { get; set; }
    /// <summary>
    /// Tỷ lệ tiết kiệm tính từ thu nhập và chi tiêu của kỳ đang xét.
    /// </summary>
    public double SavingsRate { get; set; }
    /// <summary>
    /// Số lượng cảnh báo chưa đọc để hiển thị nhắc nhở cho người dùng.
    /// </summary>
    public int UnreadAlertCount { get; set; }
    /// <summary>
    /// Danh sách ví liên quan dùng để hiển thị hoặc điều hướng dữ liệu.
    /// </summary>
    public List<WalletMiniCardViewModel> Wallets { get; set; } = [];
    /// <summary>
    /// Dữ liệu chi tiêu 7 ngày gần nhất để dựng biểu đồ cột.
    /// </summary>
    public List<ChartBarItem> SpendingSevenDays { get; set; } = [];
    /// <summary>
    /// Cơ cấu chi tiêu theo danh mục của kỳ đang chọn.
    /// </summary>
    public List<CategoryTotalItem> CategoryBreakdown { get; set; } = [];
    /// <summary>
    /// Danh sách số liệu so sánh thu chi giữa các tháng.
    /// </summary>
    public List<MonthlyComparisonItem> MonthlyComparison { get; set; } = [];
    /// <summary>
    /// Danh sách ngân sách nổi bật hoặc cần chú ý trên dashboard.
    /// </summary>
    public List<BudgetProgressViewModel> BudgetHighlights { get; set; } = [];
    /// <summary>
    /// Danh sách cảnh báo mới nhất để nhắc người dùng.
    /// </summary>
    public List<AlertItemViewModel> LatestAlerts { get; set; } = [];
    /// <summary>
    /// Danh sách giao dịch gần đây để hiển thị trên dashboard.
    /// </summary>
    public List<TransactionHistoryItemViewModel> RecentTransactions { get; set; } = [];
}
