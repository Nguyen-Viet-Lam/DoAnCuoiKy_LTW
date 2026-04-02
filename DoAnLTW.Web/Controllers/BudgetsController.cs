using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.ViewModels;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Controllers;

[Authorize(Roles = "User")]
public class BudgetsController : AppControllerBase
{
    private readonly FinanceDbContext _db;
    private readonly AuditLogService _auditLogService;

    public BudgetsController(FinanceDbContext db, AuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(int? editId, CancellationToken cancellationToken)
    {
        var model = await BuildPageModelAsync(new BudgetFormViewModel(), cancellationToken);

        if (editId.HasValue)
        {
            var budget = await _db.Budgets.FirstOrDefaultAsync(x => x.Id == editId && x.UserId == CurrentUserId, cancellationToken);
            if (budget is not null)
            {
                model.Form = new BudgetFormViewModel
                {
                    Id = budget.Id,
                    CategoryId = budget.CategoryId,
                    LimitAmount = budget.LimitAmount,
                    Month = budget.Month,
                    Year = budget.Year,
                    AlertThresholdPercent = budget.AlertThresholdPercent,
                    CategoryOptions = model.Form.CategoryOptions
                };
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([Bind(Prefix = "Form")] BudgetFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildPageModelAsync(model, cancellationToken));
        }

        var existing = await _db.Budgets
            .FirstOrDefaultAsync(
                x => x.UserId == CurrentUserId &&
                     x.CategoryId == model.CategoryId &&
                     x.Year == model.Year &&
                     x.Month == model.Month &&
                     x.Id != model.Id,
                cancellationToken);

        if (existing is not null)
        {
            ModelState.AddModelError(string.Empty, "Ngân sách cho danh mục này trong tháng đã tồn tại.");
            return View("Index", await BuildPageModelAsync(model, cancellationToken));
        }

        if (model.Id.HasValue)
        {
            var budget = await _db.Budgets.FirstOrDefaultAsync(x => x.Id == model.Id && x.UserId == CurrentUserId, cancellationToken);
            if (budget is null)
            {
                return NotFound();
            }

            budget.CategoryId = model.CategoryId;
            budget.Year = model.Year;
            budget.Month = model.Month;
            budget.LimitAmount = model.LimitAmount;
            budget.AlertThresholdPercent = model.AlertThresholdPercent;
        }
        else
        {
            _db.Budgets.Add(new Budget
            {
                UserId = CurrentUserId,
                CategoryId = model.CategoryId,
                Year = model.Year,
                Month = model.Month,
                LimitAmount = model.LimitAmount,
                AlertThresholdPercent = model.AlertThresholdPercent,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _auditLogService.WriteAsync("BudgetSave", "Lưu ngân sách tháng", CurrentUserId, cancellationToken: cancellationToken);
        TempData["SuccessMessage"] = "Đã lưu ngân sách.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var budget = await _db.Budgets.FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId, cancellationToken);
        if (budget is null)
        {
            return NotFound();
        }

        var alerts = await _db.BudgetAlerts.Where(x => x.BudgetId == budget.Id).ToListAsync(cancellationToken);
        _db.BudgetAlerts.RemoveRange(alerts);
        _db.Budgets.Remove(budget);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync("BudgetDelete", "Xóa ngân sách", CurrentUserId, cancellationToken: cancellationToken);
        TempData["SuccessMessage"] = "Đã xóa ngân sách.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<BudgetsPageViewModel> BuildPageModelAsync(BudgetFormViewModel form, CancellationToken cancellationToken)
    {
        var categories = await _db.Categories
            .Where(x => x.TransactionType == "Expense")
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        form.CategoryOptions = categories.Select(x => new SelectOptionViewModel
        {
            Value = x.Id,
            Text = x.Name,
            Extra = x.ColorHex
        }).ToList();

        if (form.CategoryId == 0 && form.CategoryOptions.Count > 0)
        {
            form.CategoryId = form.CategoryOptions.First().Value;
        }

        var budgets = await _db.Budgets
            .Include(x => x.Category)
            .Where(x => x.UserId == CurrentUserId)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync(cancellationToken);

        var items = new List<BudgetProgressViewModel>();
        foreach (var budget in budgets)
        {
            var spent = await _db.Transactions
                .Where(x => x.UserId == CurrentUserId &&
                            x.Type == "Expense" &&
                            x.CategoryId == budget.CategoryId &&
                            x.OccurredOn.Year == budget.Year &&
                            x.OccurredOn.Month == budget.Month)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            items.Add(new BudgetProgressViewModel
            {
                Id = budget.Id,
                CategoryName = budget.Category.Name,
                ColorHex = budget.Category.ColorHex,
                LimitAmount = budget.LimitAmount,
                SpentAmount = spent,
                UsagePercent = budget.LimitAmount == 0 ? 0 : (double)(spent / budget.LimitAmount * 100m),
                MonthLabel = $"{budget.Month:00}/{budget.Year}",
                AlertThresholdPercent = budget.AlertThresholdPercent
            });
        }

        return new BudgetsPageViewModel
        {
            Form = form,
            Budgets = items,
            TotalBudget = items.Sum(x => x.LimitAmount),
            TotalSpent = items.Sum(x => x.SpentAmount)
        };
    }
}
