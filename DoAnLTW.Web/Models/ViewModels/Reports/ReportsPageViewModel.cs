namespace DoAnLTW.Web.Models.ViewModels;

public class ReportsPageViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public ReportSummaryViewModel WeeklySummary { get; set; } = new();
    public ReportSummaryViewModel MonthlySummary { get; set; } = new();
    public ReportSummaryViewModel CustomSummary { get; set; } = new();
    public string DefaultEmail { get; set; } = string.Empty;
}
