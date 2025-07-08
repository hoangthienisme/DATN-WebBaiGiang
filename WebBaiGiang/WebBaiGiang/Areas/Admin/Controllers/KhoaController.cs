using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhoaController : Controller
    {
        private readonly WebBaiGiangContext _context;

        public KhoaController(WebBaiGiangContext context)
        {
            _context = context;
        }

        // GET: Danh sách khoa
        public async Task<IActionResult> Khoa(string search)
        {
            ViewBag.CurrentSearch = search;

            var query = _context.Khoas.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(k => k.Name.Contains(search));
            }

            var result = await query.OrderByDescending(k => k.CreatedDate).ToListAsync();
            return View(result);
        }

        // GET: Thêm khoa
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thêm khoa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Khoa model)
        {
         
            if (!ModelState.IsValid)
            {
                return View(model); 
            }
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
               
                ModelState.AddModelError("", "Không xác định được người tạo.");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                model.CreatedBy = userId;
                _context.Khoas.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm khoa thành công.";
                return RedirectToAction("Khoa", "Khoa", new { area = "Admin" });
            }

            return View(model);
        }

        // GET: Sửa khoa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
            {
                return NotFound();
            }
            return View(khoa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Khoa model)
        {
            if (id != model.Id)
                return NotFound();

            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Trả lại dữ liệu cũ nếu ModelState sai, giúp giữ nguyên mô tả/tên đã nhập
                return View(khoa);
            }

            try
            {
                // Lấy ID người dùng từ Claims
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    khoa.UpdateBy = userId;
                }

                // Cập nhật thông tin
                khoa.Name = model.Name;
                khoa.Description = model.Description;
                khoa.UpdateDate = DateTime.Now;

                _context.Update(khoa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật khoa thành công.";
                return RedirectToAction("Khoa", "Khoa", new { area = "Admin" });

            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Lỗi cập nhật dữ liệu.");
            }
        }


        public async Task<IActionResult> An(int id)
        {
            var khoa = await _context.Khoas
                .Include(k => k.HocPhans) // 👈 lấy luôn danh sách học phần trong khoa
                .FirstOrDefaultAsync(k => k.Id == id);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (khoa != null)
            {
                if (int.TryParse(userIdStr, out int userId))
                {
                    khoa.UpdateBy = userId;
                }

                khoa.IsActive = false;
                khoa.UpdateDate = DateTime.Now;

                // Ẩn toàn bộ học phần thuộc khoa
                foreach (var hocPhan in khoa.HocPhans)
                {
                    hocPhan.IsActive = false;
                    hocPhan.UpdateDate = DateTime.Now;
                    hocPhan.UpdateBy = khoa.UpdateBy;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Khoa", "Khoa", new { area = "Admin" });
        }


        public async Task<IActionResult> KhoiPhuc(int id)
        {
            var khoa = await _context.Khoas
                .Include(k => k.HocPhans)
                .FirstOrDefaultAsync(k => k.Id == id);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (khoa != null)
            {
                if (int.TryParse(userIdStr, out int userId))
                {
                    khoa.UpdateBy = userId;
                }

                khoa.IsActive = true;
                khoa.UpdateDate = DateTime.Now;

                // Khôi phục học phần thuộc khoa
                foreach (var hocPhan in khoa.HocPhans)
                {
                    hocPhan.IsActive = true;
                    hocPhan.UpdateDate = DateTime.Now;
                    hocPhan.UpdateBy = khoa.UpdateBy;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Khoa", "Khoa", new { area = "Admin" });
        }


    }
}
