namespace DoAnLTW.Web.Models.ViewModels;

public class WalletsPageViewModel
{
    public WalletFormViewModel Form { get; set; } = new();
    public List<WalletItemViewModel> Wallets { get; set; } = [];
    public decimal TotalBalance { get; set; }
    public decimal LowBalanceThreshold { get; set; }
}
