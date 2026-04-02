using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class WalletFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Ten vi")]
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Loai vi")]
    [Required]
    public string Type { get; set; } = "Cash";

    [Display(Name = "So du ban dau")]
    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; set; }

    [Display(Name = "Ghi chu")]
    [StringLength(250)]
    public string? Note { get; set; }
}
