using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebBaiGiang.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace WebBaiGiang.Controllers
{
    public class DiemDanhController : Controller
    {
        private readonly WebBaiGiangContext _context;

        public DiemDanhController(WebBaiGiangContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> DiemDanh(int lopIdGoc)
        {
            var danhSachPhieu = await _context.DiemDanhs
                .Include(d => d.Class)
                .Where(d => d.ClassId == lopIdGoc)
                .ToListAsync();

            ViewBag.LopIdGoc = lopIdGoc;
            return View(danhSachPhieu);
        }

        public async Task<IActionResult> TaoPhieu(int? lopIdGoc)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            // Lấy lớp mà giảng viên đang dạy
                    var classList = await _context.GiangVienLopHocs
            .Where(gl => gl.IdGv == userId)
            .Select(gl => gl.IdClassNavigation)
            .ToListAsync();

            if (classList == null)
                return NotFound("Bạn không có quyền tạo phiếu cho lớp này.");

            var viewModel = new TaoPhieuDiemDanhViewModel
            {
                LopIdGoc = lopIdGoc ?? 0,
                ClassIds = lopIdGoc.HasValue ? new List<int> { lopIdGoc.Value } : new List<int>(),
                AvailableClasses = classList.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = lopIdGoc.HasValue && c.Id == lopIdGoc.Value
                }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult TaoPhieu(TaoPhieuDiemDanhViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableClasses = _context.LopHocs.Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                }).ToList();

                return View(model);
            }
            foreach (var lopId in model.ClassIds)
            {
                var newPhieu = new DiemDanh
                {
                    ClassId = lopId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                    MaPhieu = Guid.NewGuid().ToString().Substring(0, 8),
                    ExpiredAt = model.ExpiredInMinutes.HasValue
                ? DateTime.Now.AddMinutes(model.ExpiredInMinutes.Value)
                : null

                };

                _context.DiemDanhs.Add(newPhieu);
                _context.SaveChanges();

                TempData["Message"] = $"Đã tạo phiếu điểm danh. Mã truy cập: {newPhieu.MaPhieu}";

                
            }
            return RedirectToAction("DiemDanh", new { lopIdGoc = model.ClassIds.FirstOrDefault() });

        }
        [HttpGet]
        public IActionResult ThamGia(string ma)
        {
            var phieu = _context.DiemDanhs.FirstOrDefault(p => p.MaPhieu == ma);
            if (phieu == null)
            {
                return NotFound("Phiếu điểm danh không tồn tại.");
            }

            // Hiển thị form điểm danh (checkbox hoặc nút xác nhận)
            return View(phieu);
        }

        [HttpPost]
        public IActionResult ThamGiaSubmit(string ma)
        {
            var phieu = _context.DiemDanhs.FirstOrDefault(p => p.MaPhieu == ma);
            if (phieu == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            if (!_context.ChiTietDiemDanhs.Any(dd => dd.AttendanceId == phieu.Id && dd.UsersId == userId))
            {
                _context.ChiTietDiemDanhs.Add(new ChiTietDiemDanh
                {
                    AttendanceId = phieu.Id,
                    UsersId = userId,
                    Status = "Có mặt"
                });
                _context.SaveChanges();
            }

            TempData["Message"] = "Điểm danh thành công!";
            return RedirectToAction("Index", "Home");
        }
    }
}
