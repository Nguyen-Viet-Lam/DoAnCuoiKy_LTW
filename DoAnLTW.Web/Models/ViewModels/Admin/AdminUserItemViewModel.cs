namespace DoAnLTW.Web.Models.ViewModels;

public class AdminUserItemViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public int WalletCount { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalBalance { get; set; }
}
