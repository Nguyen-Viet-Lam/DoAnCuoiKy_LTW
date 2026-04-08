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
        var periodLastDay = end.AddDays(-1);
        var today = DateTime.Today;
        if (today < start)
        {
            today = start;
        }
        else if (today > periodLastDay)
        {
            today = periodLastDay;
        }

        var userCreatedAt = await _db.Users
            .Where(x => x.Id == userId)
            .Select(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Tai khoan moi tao giua thang thi chi nen noi suy tren khoang thoi gian
        // ma he thong da co co hoi quan sat hanh vi chi tieu.
        var observationStart = userCreatedAt == default
            ? start
            : userCreatedAt.Date > start
                ? userCreatedAt.Date
                : start;

        if (observationStart > today)
        {
            observationStart = today;
        }

        var daysPassed = Math.Max(1, (today - observationStart).Days + 1);
        var daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);

        var currentMonthExpenses = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.Type == "Expense" &&
                        x.OccurredOn >= start &&
                        x.OccurredOn < end)
            .Select(x => x.Amount)
            .ToListAsync(cancellationToken);

        if (currentMonthExpenses.Count == 0)
        {
            var avgPast = await GetAverageMonthlyExpenseAsync(userId, start, cancellationToken);
            return Math.Round(avgPast, 0);
        }

        // Tách các khoản chi bất thường ra khỏi phần chi tiêu thường xuyên.
        var spikeThreshold = CalculateSpikeThreshold(currentMonthExpenses);

        decimal spikeTotal = 0m;
        decimal routineTotal = 0m;

        foreach (var amount in currentMonthExpenses)
        {
            if (amount >= spikeThreshold && spikeThreshold > 0)
            {
                spikeTotal += amount;
            }
            else
            {
                routineTotal += amount;
            }
        }

        // Nội suy phần chi tiêu thường xuyên cho toàn bộ tháng.
        var routineForecast = routineTotal / daysPassed * daysInMonth;
        var currentForecast = spikeTotal + routineForecast;

        var averagePastMonths = await GetAverageMonthlyExpenseAsync(userId, start, cancellationToken);

        if (averagePastMonths == 0m)
        {
            return Math.Round(currentForecast, 0);
        }

        // Ưu tiên dữ liệu tháng hiện tại nhưng vẫn tham chiếu lịch sử gần nhất để kết quả ổn định hơn.
        return Math.Round((currentForecast * 0.65m) + (averagePastMonths * 0.35m), 0);
    }

    private static decimal CalculateSpikeThreshold(List<decimal> amounts)
    {
        if (amounts.Count < 3)
        {
            return decimal.MaxValue;
        }

        var sorted = amounts.OrderBy(x => x).ToList();
        var median = GetPercentile(sorted, 50);

        if (sorted.First() == sorted.Last())
        {
            return decimal.MaxValue;
        }

        var q1 = GetPercentile(sorted, 25);
        var q3 = GetPercentile(sorted, 75);
        var iqr = q3 - q1;
        var iqrThreshold = q3 + 1.5m * iqr;
        var minThreshold = median * 3m;
        var threshold = Math.Max(iqrThreshold, minThreshold);

        // Giữ một ngưỡng tối thiểu để các khoản chi nhỏ nhưng lệch nhẹ không bị xem là đột biến.
        return Math.Max(threshold, 500_000m);
    }

    private static decimal GetPercentile(List<decimal> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0) return 0;
        if (sortedValues.Count == 1) return sortedValues[0];

        var index = (percentile / 100.0) * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper)
        {
            return sortedValues[lower];
        }

        var fraction = (decimal)(index - lower);
        return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
    }

    private async Task<decimal> GetAverageMonthlyExpenseAsync(int userId, DateTime currentMonthStart, CancellationToken cancellationToken)
    {
        // Lấy trung bình chi tiêu của ba tháng gần nhất để dùng làm dữ liệu tham chiếu.
        var pastThreeMonths = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.Type == "Expense" &&
                        x.OccurredOn >= currentMonthStart.AddMonths(-3) &&
                        x.OccurredOn < currentMonthStart)
            .GroupBy(x => new { x.OccurredOn.Year, x.OccurredOn.Month })
            .Select(g => g.Sum(x => x.Amount))
            .ToListAsync(cancellationToken);

        return pastThreeMonths.Count == 0 ? 0m : pastThreeMonths.Average();
    }
}
