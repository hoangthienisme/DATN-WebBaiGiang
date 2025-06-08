using Microsoft.AspNetCore.Mvc;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HocPhanController : Controller
    {
        public IActionResult HocPhan()
        {
            return View();
        }
    }
}
