namespace DoAnLTW.Web.Models.ViewModels;

public class WalletItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string? Note { get; set; }
    public int TransactionCount { get; set; }
    public bool IsLowBalance { get; set; }
}
