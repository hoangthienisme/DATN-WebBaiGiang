using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
