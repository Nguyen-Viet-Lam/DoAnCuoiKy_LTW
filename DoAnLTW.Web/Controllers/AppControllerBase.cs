using DoAnLTW.Web.Extensions;
using DoAnLTW.Web.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Web.Controllers;

[Authorize]
public abstract class AppControllerBase : Controller
{
    protected int CurrentUserId => User.GetUserId() ?? 0;

    protected void SetBudgetAlertWarning(BudgetAlertCheckResult result)
    {
        if (!result.AlertCreated || string.IsNullOrWhiteSpace(result.Message))
        {
            return;
        }

        TempData["WarningMessage"] =
            $"{result.Message} Canh bao da duoc luu trong dashboard. Email se duoc gui neu SMTP da cau hinh.";
    }
}
