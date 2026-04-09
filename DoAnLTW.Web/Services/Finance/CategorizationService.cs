using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Finance;

/// <summary>Service gợi ý danh mục giao dịch từ ghi chú và học dần theo thói quen của người dùng.</summary>
public class CategorizationService
{
    private static readonly Dictionary<string, string> ExpenseRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["xang"] = "di chuyen",
        ["grab"] = "di chuyen",
        ["taxi"] = "di chuyen",
        ["bus"] = "di chuyen",
        ["cafe"] = "an uong",
        ["tra sua"] = "an uong",
        ["com"] = "an uong",
        ["bun"] = "an uong",
        ["an"] = "an uong",
        ["dien"] = "hoa don",
        ["nuoc"] = "hoa don",
        ["internet"] = "hoa don",
        ["netflix"] = "giai tri",
        ["phim"] = "giai tri",
        ["game"] = "giai tri",
        ["thuoc"] = "suc khoe",
        ["benh vien"] = "suc khoe",
        ["hoc phi"] = "hoc tap",
        ["sach"] = "hoc tap",
        ["ao"] = "mua sam",
        ["giay"] = "mua sam"
    };

    private static readonly Dictionary<string, string> IncomeRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["luong"] = "luong",
        ["salary"] = "luong",
        ["thuong"] = "thuong",
        ["bonus"] = "thuong",
        ["ban hang"] = "ban hang",
        ["freelance"] = "thu khac",
        ["qua"] = "qua tang"
    };

    private readonly FinanceDbContext _db;

    /// <summary>
    /// Khởi tạo lớp CategorizationService và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public CategorizationService(FinanceDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Phân tích ghi chú giao dịch để gợi ý danh mục phù hợp cho người dùng.
    /// </summary>
    public async Task<CategorySuggestionResult> SuggestAsync(int userId, string transactionType, string note, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return CategorySuggestionResult.Empty;
        }

        var normalizedText = Normalize(note);
        var keywords = ExtractKeywords(normalizedText).ToArray();

        var personalKeywords = await _db.UserPersonalKeywords
            .Include(x => x.Category)
            .Where(x => x.UserId == userId && x.Category.TransactionType == transactionType)
            .OrderByDescending(x => x.HitCount)
            .ThenByDescending(x => x.LastUsedAt)
            .ToListAsync(cancellationToken);

        foreach (var keyword in keywords)
        {
            var personal = personalKeywords.FirstOrDefault(x => x.Keyword == keyword);
            if (personal is not null)
            {
                return new CategorySuggestionResult(personal.CategoryId, personal.Category.Name, keyword, "personal", 0.98);
            }
        }

        var categories = await _db.Categories
            .Where(x => x.TransactionType == transactionType)
            .ToListAsync(cancellationToken);

        var categoryLookup = categories
            .GroupBy(x => Normalize(x.Name))
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var rules = string.Equals(transactionType, "Income", StringComparison.OrdinalIgnoreCase)
            ? IncomeRules
            : ExpenseRules;

        foreach (var rule in rules)
        {
            if (!normalizedText.Contains(rule.Key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (categoryLookup.TryGetValue(rule.Value, out var category))
            {
                return new CategorySuggestionResult(category.Id, category.Name, rule.Key, "rule", 0.75);
            }
        }

        return CategorySuggestionResult.Empty;
    }

    /// <summary>
    /// Học thêm từ ghi chú và lựa chọn cuối cùng của người dùng để cải thiện gợi ý lần sau.
    /// </summary>
    public async Task LearnAsync(
        int userId,
        string note,
        int finalCategoryId,
        int? suggestedCategoryId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        if (suggestedCategoryId.HasValue && suggestedCategoryId.Value == finalCategoryId)
        {
            return;
        }

        var keywords = ExtractKeywords(Normalize(note))
            .Where(x => x.Length >= 3)
            .Take(5)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var keyword in keywords)
        {
            var existing = await _db.UserPersonalKeywords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Keyword == keyword, cancellationToken);

            if (existing is null)
            {
                _db.UserPersonalKeywords.Add(new UserPersonalKeyword
                {
                    UserId = userId,
                    Keyword = keyword,
                    CategoryId = finalCategoryId,
                    HitCount = 1,
                    LastUsedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.CategoryId = finalCategoryId;
                existing.HitCount += 1;
                existing.LastUsedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var lowered = RemoveDiacritics(input).ToLowerInvariant();
        return Regex.Replace(lowered, @"\s+", " ").Trim();
    }

    private static IEnumerable<string> ExtractKeywords(string input)
    {
        return Regex.Split(input, @"[^\p{L}\p{Nd}]+")
            .Where(x => !string.IsNullOrWhiteSpace(x));
    }

    private static string RemoveDiacritics(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);

            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(ch switch
            {
                '\u0111' => 'd',
                '\u0110' => 'D',
                _ => ch
            });
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}

/// <summary>Record mô tả kết quả gợi ý danh mục gồm nguồn gợi ý, từ khóa khớp và độ tin cậy.</summary>
public record CategorySuggestionResult(int CategoryId, string CategoryName, string MatchedKeyword, string Source, double Confidence)
{
    /// <summary>
    /// Giá trị rỗng biểu diễn trường hợp chưa gợi ý được danh mục phù hợp.
    /// </summary>
    public static CategorySuggestionResult Empty => new(0, string.Empty, string.Empty, string.Empty, 0);
    /// <summary>
    /// Cho biết kết quả gợi ý hiện tại có chứa danh mục hợp lệ hay không.
    /// </summary>
    public bool HasValue => CategoryId > 0;
}
