using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class AdminCategoryFormViewModel
{
    public int? Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string TransactionType { get; set; } = "Expense";

    [Required]
    public string Icon { get; set; } = "bi-circle";

    [Required]
    public string ColorHex { get; set; } = "#8ecae6";
}
