using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

[Authorize(Roles = "User")]
public class TransactionsController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly CategorizationService _categorizationService;
    private readonly BudgetMonitorService _budgetMonitorService;
    private readonly WalletBalanceMonitorService _walletBalanceMonitorService;
    private readonly AuditLogService _auditLogService;

    public TransactionsController(
        FinanceDbContext db,
        CategorizationService categorizationService,
        BudgetMonitorService budgetMonitorService,
        WalletBalanceMonitorService walletBalanceMonitorService,
        AuditLogService auditLogService)
    {
        _db = db;
        _categorizationService = categorizationService;
        _budgetMonitorService = budgetMonitorService;
        _walletBalanceMonitorService = walletBalanceMonitorService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(
        string type = "Expense",
        int? walletId = null,
        int? categoryId = null,
        string keyword = "",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? editId = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilterViewModel
        {
            Type = type,
            WalletId = walletId,
            CategoryId = categoryId,
            Keyword = keyword,
            FromDate = fromDate,
            ToDate = toDate
        };

        return View(await BuildPageModelAsync(new TransactionFormViewModel { Type = type }, filter, editId, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Form")] TransactionFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildPageModelAsync(model, new TransactionFilterViewModel { Type = model.Type }, model.Id, cancellationToken));
        }

        var category = await _db.Categories.FirstOrDefaultAsync(
            x => x.Id == model.CategoryId && x.TransactionType == model.Type,
            cancellationToken);

        if (category is null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Danh muc khong hop le.");
            return View("Index", await BuildPageModelAsync(model, new TransactionFilterViewModel { Type = model.Type }, model.Id, cancellationToken));
        }

        var suggestion = await _categorizationService.SuggestAsync(CurrentUserId, model.Type, model.Note, cancellationToken);
        var suggestedCategoryId = suggestion.HasValue ? (int?)suggestion.CategoryId : null;

        if (model.Id.HasValue)
        {
            var existingTransaction = await _db.Transactions
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync(x => x.Id == model.Id.Value && x.UserId == CurrentUserId, cancellationToken);

            if (existingTransaction is null)
            {
                return NotFound();
            }

            var destinationWallet = existingTransaction.WalletId == model.WalletId
                ? existingTransaction.Wallet
                : await _db.Wallets.FirstOrDefaultAsync(
                    x => x.Id == model.WalletId && x.UserId == CurrentUserId,
                    cancellationToken);

            if (destinationWallet is null)
            {
                ModelState.AddModelError(nameof(model.WalletId), "Vi khong hop le.");
                return View("Index", await BuildPageModelAsync(model, new TransactionFilterViewModel { Type = model.Type }, model.Id, cancellationToken));
            }

            var sourceWallet = existingTransaction.Wallet;
            var sourceWalletBalanceBefore = sourceWallet.CurrentBalance;
            var sameWallet = sourceWallet.Id == destinationWallet.Id;
            var destinationWalletBalanceBefore = sameWallet ? sourceWalletBalanceBefore : destinationWallet.CurrentBalance;

            ReverseWalletBalance(sourceWallet, existingTransaction.Type, existingTransaction.Amount);

            existingTransaction.WalletId = destinationWallet.Id;
            existingTransaction.CategoryId = model.CategoryId;
            existingTransaction.Type = model.Type;
            existingTransaction.Amount = model.Amount;
            existingTransaction.Note = model.Note.Trim();
            existingTransaction.OccurredOn = model.OccurredOn;
            existingTransaction.AiSuggestedCategoryId = suggestedCategoryId;
            existingTransaction.AiSuggestedLabel = suggestion.HasValue ? suggestion.CategoryName : null;
            existingTransaction.AiConfidence = suggestion.Confidence;

            ApplyWalletBalance(destinationWallet, existingTransaction.Type, existingTransaction.Amount);

            var alerts = await _db.BudgetAlerts
                .Where(x => x.WalletTransactionId == existingTransaction.Id)
                .ToListAsync(cancellationToken);
            _db.BudgetAlerts.RemoveRange(alerts);

            await _db.SaveChangesAsync(cancellationToken);
            await _categorizationService.LearnAsync(CurrentUserId, model.Note, model.CategoryId, suggestedCategoryId, cancellationToken);
            var budgetAlertResult = await _budgetMonitorService.CheckAndNotifyAsync(
                CurrentUserId,
                model.CategoryId,
                existingTransaction.Id,
                cancellationToken);
            await _walletBalanceMonitorService.NotifyIfThresholdCrossedAsync(CurrentUserId, sourceWallet, sourceWalletBalanceBefore, cancellationToken);

            if (!sameWallet)
            {
                await _walletBalanceMonitorService.NotifyIfThresholdCrossedAsync(CurrentUserId, destinationWallet, destinationWalletBalanceBefore, cancellationToken);
            }

            await _auditLogService.WriteAsync("TransactionUpdate", $"Sua giao dich #{existingTransaction.Id}", CurrentUserId, cancellationToken: cancellationToken);

            SetBudgetAlertWarning(budgetAlertResult);
            TempData["SuccessMessage"] = "Da cap nhat giao dich.";
            return RedirectToAction(nameof(Index), new { type = model.Type });
        }

        var wallet = await _db.Wallets.FirstOrDefaultAsync(
            x => x.Id == model.WalletId && x.UserId == CurrentUserId,
            cancellationToken);

        if (wallet is null)
        {
            ModelState.AddModelError(nameof(model.WalletId), "Vi khong hop le.");
            return View("Index", await BuildPageModelAsync(model, new TransactionFilterViewModel { Type = model.Type }, null, cancellationToken));
        }

        var walletBalanceBefore = wallet.CurrentBalance;
        var transaction = new WalletTransaction
        {
            UserId = CurrentUserId,
            WalletId = model.WalletId,
            CategoryId = model.CategoryId,
            Type = model.Type,
            Amount = model.Amount,
            Note = model.Note.Trim(),
            OccurredOn = model.OccurredOn,
            CreatedAt = DateTime.UtcNow,
            AiSuggestedCategoryId = suggestedCategoryId,
            AiSuggestedLabel = suggestion.HasValue ? suggestion.CategoryName : null,
            AiConfidence = suggestion.Confidence
        };

        ApplyWalletBalance(wallet, model.Type, model.Amount);
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);

        await _categorizationService.LearnAsync(CurrentUserId, model.Note, model.CategoryId, suggestedCategoryId, cancellationToken);
        var createdBudgetAlertResult = await _budgetMonitorService.CheckAndNotifyAsync(
            CurrentUserId,
            model.CategoryId,
            transaction.Id,
            cancellationToken);
        await _walletBalanceMonitorService.NotifyIfThresholdCrossedAsync(CurrentUserId, wallet, walletBalanceBefore, cancellationToken);
        await _auditLogService.WriteAsync("TransactionCreate", $"Them giao dich {transaction.Type} {transaction.Amount:N0} VND", CurrentUserId, cancellationToken: cancellationToken);

        SetBudgetAlertWarning(createdBudgetAlertResult);
        TempData["SuccessMessage"] = "Da luu giao dich.";
        return RedirectToAction(nameof(Index), new { type = model.Type });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string type = "Expense", CancellationToken cancellationToken = default)
    {
        var transaction = await _db.Transactions
            .Include(x => x.Wallet)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId, cancellationToken);

        if (transaction is null)
        {
            return NotFound();
        }

        var walletBalanceBefore = transaction.Wallet.CurrentBalance;
        ReverseWalletBalance(transaction.Wallet, transaction.Type, transaction.Amount);

        var alerts = await _db.BudgetAlerts
            .Where(x => x.WalletTransactionId == transaction.Id)
            .ToListAsync(cancellationToken);

        _db.BudgetAlerts.RemoveRange(alerts);
        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync(cancellationToken);

        await _walletBalanceMonitorService.NotifyIfThresholdCrossedAsync(CurrentUserId, transaction.Wallet, walletBalanceBefore, cancellationToken);
        await _auditLogService.WriteAsync("TransactionDelete", $"Xoa giao dich #{id}", CurrentUserId, cancellationToken: cancellationToken);
        TempData["SuccessMessage"] = "Da xoa giao dich.";
        return RedirectToAction(nameof(Index), new { type });
    }

    [HttpGet]
    public async Task<IActionResult> SuggestCategory(string type, string note, CancellationToken cancellationToken)
    {
        var result = await _categorizationService.SuggestAsync(CurrentUserId, type, note, cancellationToken);

        return Json(new
        {
            hasValue = result.HasValue,
            categoryId = result.CategoryId,
            categoryName = result.CategoryName,
            confidence = result.Confidence,
            matchedKeyword = result.MatchedKeyword,
            source = result.Source,
            explanation = !result.HasValue
                ? string.Empty
                : result.Source == "personal"
                    ? $"He thong uu tien nhan ban da tung chon cho tu khoa \"{result.MatchedKeyword}\"."
                    : $"He thong nhan ra tu khoa \"{result.MatchedKeyword}\" trong ghi chu."
        });
    }

    private async Task<TransactionsPageViewModel> BuildPageModelAsync(
        TransactionFormViewModel form,
        TransactionFilterViewModel filter,
        int? editId,
        CancellationToken cancellationToken)
    {
        filter.Type = string.IsNullOrWhiteSpace(filter.Type) ? "Expense" : filter.Type;
        form.Type = string.IsNullOrWhiteSpace(form.Type) ? filter.Type : form.Type;

        var wallets = await _db.Wallets
            .Where(x => x.UserId == CurrentUserId && !x.IsArchived)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var categories = await _db.Categories
            .Where(x => x.TransactionType == filter.Type)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var walletOptions = wallets.Select(x => new SelectOptionViewModel
        {
            Value = x.Id,
            Text = x.Name,
            Extra = x.Type
        }).ToList();

        var categoryOptions = categories.Select(x => new SelectOptionViewModel
        {
            Value = x.Id,
            Text = x.Name,
            Extra = x.ColorHex
        }).ToList();

        form.WalletOptions = walletOptions;
        form.CategoryOptions = categoryOptions;
        filter.WalletOptions = walletOptions;
        filter.CategoryOptions = categoryOptions;

        if (editId.HasValue)
        {
            var editTransaction = await _db.Transactions
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.UserId == CurrentUserId, cancellationToken);

            if (editTransaction is not null)
            {
                form = new TransactionFormViewModel
                {
                    Id = editTransaction.Id,
                    WalletId = editTransaction.WalletId,
                    CategoryId = editTransaction.CategoryId,
                    Type = editTransaction.Type,
                    Amount = editTransaction.Amount,
                    Note = editTransaction.Note,
                    OccurredOn = editTransaction.OccurredOn,
                    WalletOptions = walletOptions,
                    CategoryOptions = await _db.Categories
                        .Where(x => x.TransactionType == editTransaction.Type)
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectOptionViewModel
                        {
                            Value = x.Id,
                            Text = x.Name,
                            Extra = x.ColorHex
                        }).ToListAsync(cancellationToken)
                };

                filter.Type = editTransaction.Type;
                filter.CategoryOptions = form.CategoryOptions;
            }
        }
        else
        {
            if (form.WalletId == 0 && walletOptions.Count > 0)
            {
                form.WalletId = walletOptions.First().Value;
            }

            if (form.CategoryId == 0 && categoryOptions.Count > 0)
            {
                form.CategoryId = categoryOptions.First().Value;
            }
        }

        var now = DateTime.Today;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var query = _db.Transactions
            .Include(x => x.Category)
            .Include(x => x.Wallet)
            .Where(x => x.UserId == CurrentUserId && x.Type == filter.Type);

        if (filter.WalletId.HasValue)
        {
            query = query.Where(x => x.WalletId == filter.WalletId.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var normalizedKeyword = filter.Keyword.Trim().ToLower();
            query = query.Where(x =>
                x.Note.ToLower().Contains(normalizedKeyword) ||
                x.Category.Name.ToLower().Contains(normalizedKeyword));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.OccurredOn.Date >= filter.FromDate.Value.Date);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.OccurredOn.Date <= filter.ToDate.Value.Date);
        }

        var totalResults = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(x => x.OccurredOn)
            .Take(80)
            .ToListAsync(cancellationToken);

        return new TransactionsPageViewModel
        {
            Form = form,
            Filter = filter,
            Transactions = transactions.Select(MapTransaction).ToList(),
            Timeline = transactions.Take(12).Select(MapTransaction).ToList(),
            TotalResults = totalResults,
            TotalIncomeMonth = await _db.Transactions
                .Where(x => x.UserId == CurrentUserId && x.Type == "Income" && x.OccurredOn >= monthStart && x.OccurredOn < monthStart.AddMonths(1))
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m,
            TotalExpenseMonth = await _db.Transactions
                .Where(x => x.UserId == CurrentUserId && x.Type == "Expense" && x.OccurredOn >= monthStart && x.OccurredOn < monthStart.AddMonths(1))
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m
        };
    }

    private static void ApplyWalletBalance(Wallet wallet, string type, decimal amount)
    {
        if (type == "Expense")
        {
            wallet.CurrentBalance -= amount;
        }
        else
        {
            wallet.CurrentBalance += amount;
        }
    }

    private static void ReverseWalletBalance(Wallet wallet, string type, decimal amount)
    {
        if (type == "Expense")
        {
            wallet.CurrentBalance += amount;
        }
        else
        {
            wallet.CurrentBalance -= amount;
        }
    }

    private static TransactionHistoryItemViewModel MapTransaction(WalletTransaction transaction)
    {
        return new TransactionHistoryItemViewModel
        {
            Id = transaction.Id,
            Date = transaction.OccurredOn,
            WalletName = transaction.Wallet.Name,
            CategoryName = transaction.Category.Name,
            CategoryColor = transaction.Category.ColorHex,
            CategoryIcon = transaction.Category.Icon,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Note = transaction.Note,
            UsedAiSuggestion = transaction.AiSuggestedCategoryId == transaction.CategoryId
        };
    }
}
