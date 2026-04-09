namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp dữ liệu cho trang quản lý danh mục của admin.</summary>
public class AdminCategoriesPageViewModel
{
    /// <summary>
    /// Bộ lọc loại giao dịch đang áp dụng cho danh mục.
    /// </summary>
    public string TransactionTypeFilter { get; set; } = "All";
    /// <summary>
    /// Tổng số danh mục thuộc nhóm chi tiêu đang có trong hệ thống.
    /// </summary>
    public int TotalExpenseCategories { get; set; }
    /// <summary>
    /// Tổng số danh mục thuộc nhóm thu nhập đang có trong hệ thống.
    /// </summary>
    public int TotalIncomeCategories { get; set; }
    /// <summary>
    /// Danh sách danh mục dùng để hiển thị hoặc quản trị.
    /// </summary>
    public List<AdminCategoryItemViewModel> Categories { get; set; } = [];
    /// <summary>
    /// Dữ liệu form thêm mới hoặc chỉnh sửa danh mục ở màn hình quản trị.
    /// </summary>
    public AdminCategoryFormViewModel CategoryForm { get; set; } = new();
}
