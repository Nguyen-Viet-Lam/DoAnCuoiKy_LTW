using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form tạo hoặc chỉnh sửa ví tiền.</summary>
public class WalletFormViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Tên hiển thị của ví hoặc nguồn tiền.
    /// </summary>
    [Display(Name = "Ten vi")]
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại ví hoặc nguồn tiền, ví dụ tiền mặt hay ngân hàng.
    /// </summary>
    [Display(Name = "Loai vi")]
    [Required]
    public string Type { get; set; } = "Cash";

    /// <summary>
    /// Số dư ban đầu khi tạo ví.
    /// </summary>
    [Display(Name = "So du ban dau")]
    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; set; }

    /// <summary>
    /// Ghi chú bổ sung cho ví hoặc nguồn tiền.
    /// </summary>
    [Display(Name = "Ghi chu")]
    [StringLength(250)]
    public string? Note { get; set; }
}
