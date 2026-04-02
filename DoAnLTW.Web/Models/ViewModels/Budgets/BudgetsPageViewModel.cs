namespace DoAnLTW.Web.Models.ViewModels;

public class BudgetsPageViewModel
{
    public BudgetFormViewModel Form { get; set; } = new();
    public List<BudgetProgressViewModel> Budgets { get; set; } = [];
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
}
