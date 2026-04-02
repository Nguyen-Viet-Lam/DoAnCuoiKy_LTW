namespace DoAnLTW.Web.Models.ViewModels;

public class MonthlyComparisonItem
{
    public string Label { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }

    public string ShortLabel => $"T{Month}";
    public string FullLabel => $"Tháng {Month}/{Year}";
    public decimal NetAmount => Income - Expense;
}
