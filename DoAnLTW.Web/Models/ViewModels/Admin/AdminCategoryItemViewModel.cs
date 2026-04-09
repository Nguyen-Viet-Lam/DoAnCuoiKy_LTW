namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model mô tả một danh mục hiển thị trong danh sách quản trị.</summary>
public class AdminCategoryItemViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Tên danh mục hiển thị cho người dùng.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Loại danh mục áp dụng cho giao dịch thu hoặc chi.
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    /// <summary>
    /// Tên icon dùng để minh họa trên giao diện.
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>
    /// Mã màu hiển thị dùng cho danh mục, thẻ hoặc biểu đồ.
    /// </summary>
    public string ColorHex { get; set; } = string.Empty;
    /// <summary>
    /// Cho biết đây có phải danh mục mặc định của hệ thống hay không.
    /// </summary>
    public bool IsDefault { get; set; }
    /// <summary>
    /// Tổng số giao dịch liên quan đến bản ghi hoặc thống kê hiện tại.
    /// </summary>
    public int TransactionCount { get; set; }
    /// <summary>
    /// Tổng số ngân sách liên quan đến danh mục này.
    /// </summary>
    public int BudgetCount { get; set; }
}
