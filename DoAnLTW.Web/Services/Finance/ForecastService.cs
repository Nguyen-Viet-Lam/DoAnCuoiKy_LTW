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

    /// <summary>
    /// Dự báo chi tiêu cuối tháng.
    /// 
    /// Thuật toán:
    /// 1. Phân loại giao dịch tháng hiện tại thành "chi lớn bất thường" (spike) và "chi thường ngày" (routine).
    ///    - Spike: giao dịch có amount >= ngưỡng spike (median * 5 hoặc top 95th percentile).
    ///      Ví dụ: tiền trọ, tiền học — thường chỉ đóng 1 lần/tháng.
    ///    - Routine: tất cả giao dịch còn lại (ăn uống, đi lại, mua sắm nhỏ...).
    /// 
    /// 2. Dự báo = spike giữ nguyên (không nhân lên) + routine ngoại suy theo run-rate.
    /// 
    /// 3. Kết hợp với trung bình 3 tháng trước để làm mượt.
    /// </summary>
    public async Task<decimal> CalculateMonthEndExpenseForecastAsync(int userId, DateTime month, CancellationToken cancellationToken = default)
    {
        var start = new DateTime(month.Year, month.Month, 1);
        var end = start.AddMonths(1);
        var today = DateTime.Today < start ? start : DateTime.Today;
        var daysPassed = Math.Max(1, today.Day);
        var daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);

        // Lấy tất cả giao dịch chi tiêu tháng hiện tại
        var currentMonthExpenses = await _db.Transactions
            .Where(x => x.UserId == userId &&
                        x.Type == "Expense" &&
                        x.OccurredOn >= start &&
                        x.OccurredOn < end)
            .Select(x => x.Amount)
            .ToListAsync(cancellationToken);

        if (currentMonthExpenses.Count == 0)
        {
            // Chưa có giao dịch nào tháng này, dùng trung bình 3 tháng trước
            var avgPast = await GetAverageMonthlyExpenseAsync(userId, start, cancellationToken);
            return Math.Round(avgPast, 0);
        }

        var totalSpent = currentMonthExpenses.Sum();

        // Tính ngưỡng spike: giao dịch bất thường lớn
        var spikeThreshold = CalculateSpikeThreshold(currentMonthExpenses);

        // Tách spike và routine
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

        // Spike giữ nguyên (không nhân lên) vì tiền trọ, tiền học chỉ đóng 1 lần/tháng
        // Routine ngoại suy theo run-rate: (routine / ngày đã qua) * tổng ngày trong tháng
        var routineForecast = routineTotal / daysPassed * daysInMonth;

        var currentForecast = spikeTotal + routineForecast;

        // Kết hợp với lịch sử 3 tháng trước để làm mượt
        var averagePastMonths = await GetAverageMonthlyExpenseAsync(userId, start, cancellationToken);

        if (averagePastMonths == 0m)
        {
            return Math.Round(currentForecast, 0);
        }

        // Trọng số: ưu tiên dữ liệu tháng hiện tại (65%) + lịch sử (35%)
        return Math.Round((currentForecast * 0.65m) + (averagePastMonths * 0.35m), 0);
    }

    /// <summary>
    /// Tính ngưỡng để phân biệt chi tiêu "spike" (bất thường lớn) và "routine" (thường ngày).
    /// 
    /// Dùng IQR (Interquartile Range) — phương pháp thống kê chuẩn phát hiện outlier:
    /// - Spike = amount > Q3 + 1.5 * IQR
    /// - Nhưng ngưỡng tối thiểu = median * 3 để tránh coi chi tiêu hơi lớn là spike.
    /// 
    /// Nếu ít hơn 3 giao dịch hoặc tất cả giao dịch giống nhau → không phát hiện spike.
    /// </summary>
    private static decimal CalculateSpikeThreshold(List<decimal> amounts)
    {
        if (amounts.Count < 3)
        {
            return decimal.MaxValue; // Quá ít dữ liệu → không tách spike
        }

        var sorted = amounts.OrderBy(x => x).ToList();
        var median = GetPercentile(sorted, 50);

        // Nếu tất cả giống nhau → không có spike
        if (sorted.First() == sorted.Last())
        {
            return decimal.MaxValue;
        }

        var q1 = GetPercentile(sorted, 25);
        var q3 = GetPercentile(sorted, 75);
        var iqr = q3 - q1;

        // Ngưỡng IQR chuẩn: Q3 + 1.5 * IQR
        var iqrThreshold = q3 + 1.5m * iqr;

        // Ngưỡng tối thiểu: median * 3 (tránh false positive)
        var minThreshold = median * 3m;

        // Lấy giá trị lớn hơn giữa 2 ngưỡng
        var threshold = Math.Max(iqrThreshold, minThreshold);

        // Đảm bảo ngưỡng hợp lý (ít nhất 500,000 VND)
        return Math.Max(threshold, 500_000m);
    }

    /// <summary>
    /// Tính percentile theo nội suy tuyến tính.
    /// </summary>
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
