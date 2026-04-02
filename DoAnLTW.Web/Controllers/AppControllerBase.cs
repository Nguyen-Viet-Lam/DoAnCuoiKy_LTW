using DoAnLTW.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Web.Controllers;

[Authorize]
public abstract class AppControllerBase : Controller
{
    protected int CurrentUserId => User.GetUserId() ?? 0;
}
