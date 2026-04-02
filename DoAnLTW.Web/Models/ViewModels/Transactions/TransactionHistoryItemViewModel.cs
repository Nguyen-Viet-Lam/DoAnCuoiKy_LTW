namespace DoAnLTW.Web.Models.ViewModels;

public class TransactionHistoryItemViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string WalletName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#8ecae6";
    public string CategoryIcon { get; set; } = "bi-circle";
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool UsedAiSuggestion { get; set; }
}
