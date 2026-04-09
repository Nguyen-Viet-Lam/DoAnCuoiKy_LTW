using Microsoft.AspNetCore.Mvc;

namespace DoAnLTW.Web.Controllers;

/// <summary>Controller trang chủ và trang lỗi trạng thái HTTP.</summary>
public class HomeController : Controller
{
    /// <summary>
    /// Hiển thị trang chủ của ứng dụng.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Hiển thị trang lỗi tương ứng với mã trạng thái HTTP vừa phát sinh.
    /// </summary>
    public IActionResult ErrorStatus(int code)
    {
        ViewBag.StatusCode = code;
        return View("Error");
    }
}
