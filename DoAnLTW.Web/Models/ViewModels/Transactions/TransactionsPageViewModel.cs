namespace DoAnLTW.Web.Models.ViewModels;

public class TransactionsPageViewModel
{
    public TransactionFormViewModel Form { get; set; } = new();
    public TransactionFilterViewModel Filter { get; set; } = new();
    public List<TransactionHistoryItemViewModel> Transactions { get; set; } = [];
    public List<TransactionHistoryItemViewModel> Timeline { get; set; } = [];
    public decimal TotalIncomeMonth { get; set; }
    public decimal TotalExpenseMonth { get; set; }
    public int TotalResults { get; set; }
}
