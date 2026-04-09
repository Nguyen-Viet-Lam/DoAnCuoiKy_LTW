using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form tạo hoặc chỉnh sửa giao dịch thu chi.</summary>
public class TransactionFormViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Mã ví được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    [Display(Name = "Vi")]
    [Required]
    public int WalletId { get; set; }

    /// <summary>
    /// Mã danh mục được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    [Display(Name = "Danh muc")]
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Loại giao dịch, thường là Income hoặc Expense.
    /// </summary>
    [Display(Name = "Loai giao dich")]
    [Required]
    public string Type { get; set; } = "Expense";

    /// <summary>
    /// Số tiền của giao dịch, thống kê hoặc mục dữ liệu tương ứng.
    /// </summary>
    [Display(Name = "So tien")]
    [Range(1, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Ghi chú mô tả nội dung giao dịch.
    /// </summary>
    [Display(Name = "Ghi chu")]
    [Required, StringLength(300)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Ngày giao dịch thực tế phát sinh.
    /// </summary>
    [Display(Name = "Ngay giao dich")]
    public DateTime OccurredOn { get; set; } = DateTime.Now;

    /// <summary>
    /// Mã danh mục mà service gợi ý tự động đề xuất từ ghi chú giao dịch.
    /// </summary>
    public int? SuggestedCategoryId { get; set; }
    /// <summary>
    /// Tên danh mục được gợi ý để người dùng dễ kiểm tra trên form.
    /// </summary>
    public string? SuggestedCategoryName { get; set; }
    /// <summary>
    /// Độ tin cậy của gợi ý danh mục do bộ phân loại trả về.
    /// </summary>
    public double? SuggestedConfidence { get; set; }

    /// <summary>
    /// Danh sách ví dùng để đổ vào ô chọn trên form.
    /// </summary>
    public List<SelectOptionViewModel> WalletOptions { get; set; } = [];
    /// <summary>
    /// Danh sách danh mục dùng để đổ vào ô chọn trên form.
    /// </summary>
    public List<SelectOptionViewModel> CategoryOptions { get; set; } = [];
}
