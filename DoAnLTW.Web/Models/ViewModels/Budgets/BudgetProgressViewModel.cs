namespace DoAnLTW.Web.Models.ViewModels;

public class BudgetProgressViewModel
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#8ecae6";
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public double UsagePercent { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int AlertThresholdPercent { get; set; }
}
