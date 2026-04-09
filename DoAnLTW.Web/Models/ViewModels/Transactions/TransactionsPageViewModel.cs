namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp form, bộ lọc, danh sách và chỉ số cho trang giao dịch.</summary>
public class TransactionsPageViewModel
{
    /// <summary>
    /// Dữ liệu form đang được bind và hiển thị trên trang hiện tại.
    /// </summary>
    public TransactionFormViewModel Form { get; set; } = new();
    /// <summary>
    /// Bộ điều kiện lọc mà trang hiện tại đang sử dụng.
    /// </summary>
    public TransactionFilterViewModel Filter { get; set; } = new();
    /// <summary>
    /// Danh sách giao dịch liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public List<TransactionHistoryItemViewModel> Transactions { get; set; } = [];
    /// <summary>
    /// Dữ liệu giao dịch hiển thị theo trục thời gian trên trang.
    /// </summary>
    public List<TransactionHistoryItemViewModel> Timeline { get; set; } = [];
    /// <summary>
    /// Tổng thu của tháng đang được chọn trên giao diện.
    /// </summary>
    public decimal TotalIncomeMonth { get; set; }
    /// <summary>
    /// Tổng chi của tháng đang được chọn trên giao diện.
    /// </summary>
    public decimal TotalExpenseMonth { get; set; }
    /// <summary>
    /// Tổng số kết quả giao dịch thỏa bộ lọc hiện tại.
    /// </summary>
    public int TotalResults { get; set; }
}
