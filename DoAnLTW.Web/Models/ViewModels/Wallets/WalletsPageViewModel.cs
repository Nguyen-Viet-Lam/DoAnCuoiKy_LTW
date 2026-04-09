namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp dữ liệu cho trang danh sách ví và form thao tác với ví.</summary>
public class WalletsPageViewModel
{
    /// <summary>
    /// Dữ liệu form đang được bind và hiển thị trên trang hiện tại.
    /// </summary>
    public WalletFormViewModel Form { get; set; } = new();
    /// <summary>
    /// Danh sách ví liên quan dùng để hiển thị hoặc điều hướng dữ liệu.
    /// </summary>
    public List<WalletItemViewModel> Wallets { get; set; } = [];
    /// <summary>
    /// Tổng số dư cộng dồn của các ví đang được hiển thị.
    /// </summary>
    public decimal TotalBalance { get; set; }
    /// <summary>
    /// Ngưỡng số dư thấp dùng để tô cảnh báo trên giao diện.
    /// </summary>
    public decimal LowBalanceThreshold { get; set; }
}
