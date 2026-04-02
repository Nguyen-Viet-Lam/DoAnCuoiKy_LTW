namespace DoAnLTW.Web.Models.ViewModels;

public class AdminLogsPageViewModel
{
    public string LevelFilter { get; set; } = "All";
    public string SearchTerm { get; set; } = string.Empty;
    public int TotalLogs { get; set; }
    public int ErrorLogs { get; set; }
    public int UserActionLogs { get; set; }
    public List<SystemLogRowViewModel> Logs { get; set; } = [];
}
