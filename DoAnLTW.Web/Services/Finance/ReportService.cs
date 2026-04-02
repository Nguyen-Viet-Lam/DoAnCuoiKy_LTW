using ClosedXML.Excel;
using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

public class ReportService
{
    private readonly FinanceDbContext _db;

    public ReportService(FinanceDbContext db)
    {
        _db = db;
    }

    public async Task<ReportSummaryViewModel> BuildSummaryAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var endExclusive = to.Date.AddDays(1);
        var totalDays = Math.Max((to.Date - from.Date).Days + 1, 1);

        var transactions = await _db.Transactions
            .Include(x => x.Category)
            .Include(x => x.Wallet)
            .Where(x => x.UserId == userId && x.OccurredOn >= from.Date && x.OccurredOn < endExclusive)
            .OrderByDescending(x => x.OccurredOn)
            .ToListAsync(cancellationToken);

        var totalIncome = transactions.Where(x => x.Type == "Income").Sum(x => x.Amount);
        var totalExpense = transactions.Where(x => x.Type == "Expense").Sum(x => x.Amount);

        return new ReportSummaryViewModel
        {
            From = from.Date,
            To = to.Date,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            BalanceChange = totalIncome - totalExpense,
            TransactionCount = transactions.Count,
            AverageExpensePerDay = Math.Round(totalExpense / totalDays, 0),
            TopCategories = transactions
                .Where(x => x.Type == "Expense")
                .GroupBy(x => new { x.Category.Name, x.Category.ColorHex })
                .Select(g => new CategoryTotalItem
                {
                    CategoryName = g.Key.Name,
                    Amount = g.Sum(x => x.Amount),
                    ColorHex = g.Key.ColorHex
                })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToList(),
            Transactions = transactions
                .Select(x => new ReportTransactionItem
                {
                    Date = x.OccurredOn,
                    WalletName = x.Wallet.Name,
                    CategoryName = x.Category.Name,
                    Type = x.Type,
                    Amount = x.Amount,
                    Note = x.Note
                })
                .ToList()
        };
    }

    public async Task<byte[]> ExportExcelAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var summary = await BuildSummaryAsync(userId, from, to, cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("BaoCao");

        ws.Cell("A1").Value = "Bao cao chi tieu";
        ws.Cell("A2").Value = $"Tu {summary.From:dd/MM/yyyy} den {summary.To:dd/MM/yyyy}";
        ws.Cell("A4").Value = "Tong thu";
        ws.Cell("B4").Value = summary.TotalIncome;
        ws.Cell("A5").Value = "Tong chi";
        ws.Cell("B5").Value = summary.TotalExpense;
        ws.Cell("A6").Value = "Chenh lech";
        ws.Cell("B6").Value = summary.BalanceChange;
        ws.Cell("A7").Value = "So giao dich";
        ws.Cell("B7").Value = summary.TransactionCount;

        ws.Cell("A9").Value = "Ngay";
        ws.Cell("B9").Value = "Loai";
        ws.Cell("C9").Value = "Danh muc";
        ws.Cell("D9").Value = "Vi";
        ws.Cell("E9").Value = "So tien";
        ws.Cell("F9").Value = "Ghi chu";

        var row = 10;
        foreach (var item in summary.Transactions)
        {
            ws.Cell(row, 1).Value = item.Date;
            ws.Cell(row, 2).Value = item.Type;
            ws.Cell(row, 3).Value = item.CategoryName;
            ws.Cell(row, 4).Value = item.WalletName;
            ws.Cell(row, 5).Value = item.Amount;
            ws.Cell(row, 6).Value = item.Note;
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.Column(1).Style.DateFormat.Format = "dd/MM/yyyy";
        ws.Column(5).Style.NumberFormat.Format = "#,##0";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<string> BuildWeeklyEmailAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var summary = await BuildSummaryAsync(userId, from, to, cancellationToken);
        var topCategoryHtml = string.Join(
            string.Empty,
            summary.TopCategories.Select(x => $"<li>{x.CategoryName}: <strong>{x.Amount:N0} đ</strong></li>"));

        return $"""
                <div style="font-family:Segoe UI,Arial,sans-serif;padding:24px;color:#1f2937">
                    <h2 style="color:#2563eb;margin-bottom:8px">Báo cáo chi tiêu</h2>
                    <p>Từ {summary.From:dd/MM/yyyy} đến {summary.To:dd/MM/yyyy}</p>
                    <div style="display:flex;gap:12px;flex-wrap:wrap;margin:16px 0">
                        <div style="background:#ecfeff;border-radius:14px;padding:14px 18px;min-width:160px">
                            <div style="font-size:12px;color:#64748b">Tổng thu</div>
                            <div style="font-size:22px;font-weight:700">{summary.TotalIncome:N0} đ</div>
                        </div>
                        <div style="background:#fff7ed;border-radius:14px;padding:14px 18px;min-width:160px">
                            <div style="font-size:12px;color:#64748b">Tổng chi</div>
                            <div style="font-size:22px;font-weight:700">{summary.TotalExpense:N0} đ</div>
                        </div>
                        <div style="background:#eff6ff;border-radius:14px;padding:14px 18px;min-width:160px">
                            <div style="font-size:12px;color:#64748b">Chênh lệch</div>
                            <div style="font-size:22px;font-weight:700">{summary.BalanceChange:N0} đ</div>
                        </div>
                    </div>
                    <div style="background:#f8fbff;border-radius:14px;padding:14px 18px;margin-bottom:16px">
                        <div style="font-size:12px;color:#64748b">Số giao dịch</div>
                        <div style="font-size:22px;font-weight:700">{summary.TransactionCount}</div>
                        <div style="margin-top:8px;color:#64748b">Chi trung bình mỗi ngày: <strong>{summary.AverageExpensePerDay:N0} đ</strong></div>
                    </div>
                    <h3>Danh mục chi nhiều nhất</h3>
                    <ul>{topCategoryHtml}</ul>
                </div>
                """;
    }
}
