using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel;

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
                .Select(u => new SinhVienVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return View(sinhViens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.Id == id && x.Role == "Student");

            if (user == null)
            {
                TempData["ErrorMessage"] = "Sinh viên không tồn tại.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            user.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái tài khoản thành công.";
            return RedirectToAction("Index");
        }


    }
}
