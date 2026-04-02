namespace DoAnLTW.Web.Models.ViewModels;

public class DashboardViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public decimal TotalBalance { get; set; }
    public decimal LowBalanceThreshold { get; set; }
    public int SelectedMonth { get; set; }
    public int SelectedYear { get; set; }
    public string SelectedMonthLabel { get; set; } = string.Empty;
    public bool IsCurrentMonthSelected { get; set; }
    public decimal TotalIncomeMonth { get; set; }
    public decimal TotalExpenseMonth { get; set; }
    public decimal ForecastExpense { get; set; }
    public double SavingsRate { get; set; }
    public int UnreadAlertCount { get; set; }
    public List<WalletMiniCardViewModel> Wallets { get; set; } = [];
    public List<ChartBarItem> SpendingSevenDays { get; set; } = [];
    public List<CategoryTotalItem> CategoryBreakdown { get; set; } = [];
    public List<MonthlyComparisonItem> MonthlyComparison { get; set; } = [];
    public List<BudgetProgressViewModel> BudgetHighlights { get; set; } = [];
    public List<AlertItemViewModel> LatestAlerts { get; set; } = [];
    public List<TransactionHistoryItemViewModel> RecentTransactions { get; set; } = [];
}
