using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form tạo hoặc chỉnh sửa ngân sách.</summary>
public class BudgetFormViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Mã danh mục được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    [Display(Name = "Danh muc")]
    [Required]
    public int CategoryId { get; set; }

    /// <summary>
    /// Năm áp dụng của ngân sách, thống kê hoặc khoảng dữ liệu.
    /// </summary>
    [Display(Name = "Nam")]
    public int Year { get; set; } = DateTime.Today.Year;
    /// <summary>
    /// Tháng áp dụng của ngân sách, thống kê hoặc khoảng dữ liệu.
    /// </summary>
    [Display(Name = "Thang")]
    public int Month { get; set; } = DateTime.Today.Month;

    /// <summary>
    /// Hạn mức chi tối đa được đặt ra cho kỳ ngân sách.
    /// </summary>
    [Display(Name = "Han muc")]
    [Range(1, double.MaxValue)]
    public decimal LimitAmount { get; set; }

    /// <summary>
    /// Ngưỡng phần trăm cảnh báo được cấu hình cho ngân sách.
    /// </summary>
    [Display(Name = "Moc canh bao (%)")]
    [Range(50, 100)]
    public int AlertThresholdPercent { get; set; } = 80;

    /// <summary>
    /// Danh sách danh mục dùng để đổ vào ô chọn trên form.
    /// </summary>
    public List<SelectOptionViewModel> CategoryOptions { get; set; } = [];
}
