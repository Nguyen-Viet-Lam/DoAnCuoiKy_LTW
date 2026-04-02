namespace DoAnLTW.Web.Models.ViewModels;

public class AdminDashboardPageViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalTransactions { get; set; }
    public int NewUsersToday { get; set; }
    public int TotalCategories { get; set; }
    public List<AdminDailyStatViewModel> DailyStats { get; set; } = [];
    public List<AdminUserItemViewModel> LatestUsers { get; set; } = [];
    public List<SystemLogRowViewModel> LatestLogs { get; set; } = [];
}
