namespace DoAnLTW.Web.Models.ViewModels;

public class AdminCategoryItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int TransactionCount { get; set; }
    public int BudgetCount { get; set; }
}
