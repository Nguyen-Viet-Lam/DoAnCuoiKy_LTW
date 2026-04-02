using DoAnLTW.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

public class ForecastService
{
    private readonly FinanceDbContext _db;

    public ForecastService(FinanceDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> CalculateMonthEndExpenseForecastAsync(int userId, DateTime month, CancellationToken cancellationToken = default)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        var today = DateTime.Today < start ? start : DateTime.Today;
        var daysPassed = Math.Max(1, today.Day);

        var spentThisMonth = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.Type == "Expense" &&
                        x.OccurredOn >= start &&
                        x.OccurredOn < end)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var pastThreeMonths = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.Type == "Expense" &&
                        x.OccurredOn >= start.AddMonths(-3) &&
                        x.OccurredOn < start)
            .GroupBy(x => new { x.OccurredOn.Year, x.OccurredOn.Month })
            .Select(g => g.Sum(x => x.Amount))
            .ToListAsync(cancellationToken);

        var averagePastMonths = pastThreeMonths.Count == 0 ? 0m : pastThreeMonths.Average();
        var currentRunRateForecast = spentThisMonth / daysPassed * DateTime.DaysInMonth(start.Year, start.Month);

        if (averagePastMonths == 0m)
        {
            return Math.Round(currentRunRateForecast, 0);
        }

        return Math.Round((currentRunRateForecast * 0.65m) + (averagePastMonths * 0.35m), 0);
    }
}
