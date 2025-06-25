using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Areas.Admin.Data;
using WebBaiGiang.Models;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GiangVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<GiangVienController> _logger;
        public GiangVienController(WebBaiGiangContext context, ILogger<GiangVienController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 📄 Hiển thị danh sách giảng viên
        public async Task<IActionResult> Index()
        {
            var giangViens = await _context.NguoiDungs
                                           .Where(u => u.Role == "Teacher")
                                           .OrderByDescending(u => u.CreatedDate)
                                           .ToListAsync();
            return View(giangViens);
        }
        [HttpGet]
        // ➕ Hiển thị form tạo giảng viên
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguoiDung user)
        {
            if (ModelState.IsValid)
            {
                user.Role = "Teacher";
                user.CreatedDate = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Admin/GiangVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null || user.Role != "Teacher") return NotFound();

            var model = new EditGiangVienViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };

            return View(model);
        }

        // POST: Admin/GiangVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditGiangVienViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs.FindAsync(id);
                if (user == null || user.Role != "Teacher") return NotFound();

                user.Name = model.Name;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTrangThai(int id)
        {
            var user = _context.NguoiDungs.FirstOrDefault(x => x.Id == id && x.Role == "Teacher");
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
