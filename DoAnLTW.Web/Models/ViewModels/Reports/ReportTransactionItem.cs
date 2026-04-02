namespace DoAnLTW.Web.Models.ViewModels;

public class ReportTransactionItem
{
    public DateTime Date { get; set; }
    public string WalletName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
}
