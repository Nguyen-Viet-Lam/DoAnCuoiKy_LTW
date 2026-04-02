namespace DoAnLTW.Web.Models.ViewModels;

public class ReportSummaryViewModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal BalanceChange { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageExpensePerDay { get; set; }
    public List<CategoryTotalItem> TopCategories { get; set; } = [];
    public List<ReportTransactionItem> Transactions { get; set; } = [];
}
