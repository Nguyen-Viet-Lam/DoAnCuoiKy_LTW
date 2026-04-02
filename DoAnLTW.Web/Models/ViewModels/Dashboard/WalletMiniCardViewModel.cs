namespace DoAnLTW.Web.Models.ViewModels;

public class WalletMiniCardViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool IsLowBalance { get; set; }
}
