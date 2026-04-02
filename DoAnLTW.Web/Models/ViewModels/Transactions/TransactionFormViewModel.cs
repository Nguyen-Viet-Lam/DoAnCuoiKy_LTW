using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class TransactionFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Vi")]
    [Required]
    public int WalletId { get; set; }

    [Display(Name = "Danh muc")]
    [Required]
    public int CategoryId { get; set; }

    [Display(Name = "Loai giao dich")]
    [Required]
    public string Type { get; set; } = "Expense";

    [Display(Name = "So tien")]
    [Range(1, double.MaxValue)]
    public decimal Amount { get; set; }

    [Display(Name = "Ghi chu")]
    [Required, StringLength(300)]
    public string Note { get; set; } = string.Empty;

    [Display(Name = "Ngay giao dich")]
    public DateTime OccurredOn { get; set; } = DateTime.Now;

    public int? SuggestedCategoryId { get; set; }
    public string? SuggestedCategoryName { get; set; }
    public double? SuggestedConfidence { get; set; }

    public List<SelectOptionViewModel> WalletOptions { get; set; } = [];
    public List<SelectOptionViewModel> CategoryOptions { get; set; } = [];
}
