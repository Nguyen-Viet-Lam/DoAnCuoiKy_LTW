namespace DoAnLTW.Web.Models.ViewModels;

public class AdminCategoriesPageViewModel
{
    public string TransactionTypeFilter { get; set; } = "All";
    public int TotalExpenseCategories { get; set; }
    public int TotalIncomeCategories { get; set; }
    public List<AdminCategoryItemViewModel> Categories { get; set; } = [];
    public AdminCategoryFormViewModel CategoryForm { get; set; } = new();
}
