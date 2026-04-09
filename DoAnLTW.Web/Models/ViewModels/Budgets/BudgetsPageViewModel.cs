namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp dữ liệu cho trang quản lý ngân sách.</summary>
public class BudgetsPageViewModel
{
    /// <summary>
    /// Dữ liệu form đang được bind và hiển thị trên trang hiện tại.
    /// </summary>
    public BudgetFormViewModel Form { get; set; } = new();
    /// <summary>
    /// Danh sách ngân sách liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public List<BudgetProgressViewModel> Budgets { get; set; } = [];
    /// <summary>
    /// Tổng hạn mức ngân sách của các mục đang hiển thị trên trang.
    /// </summary>
    public decimal TotalBudget { get; set; }
    /// <summary>
    /// Tổng số tiền đã chi tương ứng với các ngân sách đang hiển thị.
    /// </summary>
    public decimal TotalSpent { get; set; }
}
