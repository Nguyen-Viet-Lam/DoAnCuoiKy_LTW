namespace DoAnLTW.Web.Models.ViewModels;

public class AdminUsersPageViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = "All";
    public string RoleFilter { get; set; } = "All";
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public List<AdminUserItemViewModel> Users { get; set; } = [];
}
