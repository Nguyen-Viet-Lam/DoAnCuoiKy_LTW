namespace DoAnLTW.Web.Models.ViewModels;

public class TransactionFilterViewModel
{
    public string Type { get; set; } = "Expense";
    public int? WalletId { get; set; }
    public int? CategoryId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public List<SelectOptionViewModel> WalletOptions { get; set; } = [];
    public List<SelectOptionViewModel> CategoryOptions { get; set; } = [];
}
