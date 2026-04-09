using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form tạo hoặc chỉnh sửa danh mục ở khu vực quản trị.</summary>
public class AdminCategoryFormViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Tên danh mục hiển thị cho người dùng.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại danh mục áp dụng cho giao dịch thu hoặc chi.
    /// </summary>
    [Required]
    public string TransactionType { get; set; } = "Expense";

    /// <summary>
    /// Tên icon dùng để minh họa trên giao diện.
    /// </summary>
    [Required]
    public string Icon { get; set; } = "bi-circle";

    /// <summary>
    /// Mã màu hiển thị dùng cho danh mục, thẻ hoặc biểu đồ.
    /// </summary>
    [Required]
    public string ColorHex { get; set; } = "#8ecae6";
}
