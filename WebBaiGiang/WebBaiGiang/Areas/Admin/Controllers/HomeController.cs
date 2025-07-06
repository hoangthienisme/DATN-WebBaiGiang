using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<HomeController> _logger;
        public HomeController(WebBaiGiangContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            ViewBag.TongGiangVien = _context.NguoiDungs.Count(x => x.Role == "Teacher");
            ViewBag.TongSinhVien = _context.NguoiDungs.Count(x => x.Role == "Student");
            ViewBag.TongKhoa = _context.Khoas.Count();
            ViewBag.TongHocPhan = _context.HocPhans.Count();

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
