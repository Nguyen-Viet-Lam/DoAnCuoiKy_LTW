using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class BudgetFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Danh muc")]
    [Required]
    public int CategoryId { get; set; }

    [Display(Name = "Nam")]
    public int Year { get; set; } = DateTime.Today.Year;
    [Display(Name = "Thang")]
    public int Month { get; set; } = DateTime.Today.Month;

    [Display(Name = "Han muc")]
    [Range(1, double.MaxValue)]
    public decimal LimitAmount { get; set; }

    [Display(Name = "Moc canh bao (%)")]
    [Range(50, 100)]
    public int AlertThresholdPercent { get; set; } = 80;

    public List<SelectOptionViewModel> CategoryOptions { get; set; } = [];
}
