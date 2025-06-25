using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SinhVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<SinhVienController> _logger;
        public SinhVienController(WebBaiGiangContext context, ILogger<SinhVienController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var sinhViens = await _context.NguoiDungs
                                            .Where(u => u.Role == "Student")
                                            .OrderByDescending(u => u.CreatedDate)
                                            .ToListAsync();
            return View(sinhViens);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTrangThai(int id)
        {
            var user = _context.NguoiDungs.FirstOrDefault(x => x.Id == id && x.Role == "Student");
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            user.UpdateDate = DateTime.Now;
            // Bạn có thể thêm user.UpdateBy = ... nếu cần
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
