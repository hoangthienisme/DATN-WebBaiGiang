using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel;

namespace WebBaiGiang.Controllers
{
    [Authorize(Roles = "Student")]
    public class SinhVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _env;
        public SinhVienController(WebBaiGiangContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Courses(string? search, int page = 1)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            // Truy vấn gốc
            var myCoursesQuery = _context.LopHocs
                .Where(l => l.IsActive && l.SinhVienLopHocs.Any(gv => gv.IdSv == userId));

            // Nếu có tìm kiếm, lọc theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(search))
            {
                string lowerSearch = search.ToLower();
                myCoursesQuery = myCoursesQuery.Where(l =>
                    l.Name.ToLower().Contains(lowerSearch) ||
                    (l.Description != null && l.Description.ToLower().Contains(lowerSearch)));
            }

            // Sắp xếp và chọn các trường cần
            myCoursesQuery = myCoursesQuery
                .OrderByDescending(l => l.CreatedDate)
                .Select(l => new LopHoc
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Picture = l.Picture,
                    CreatedDate = l.CreatedDate,
                    JoinCode = l.JoinCode
                });

            var paginatedCourses = await PhanTrang<LopHoc>.CreateAsync(myCoursesQuery, page, pageSize);
            ViewBag.Search = search;

            return View(paginatedCourses);
        }

        public async Task<IActionResult> DetailCourses(int id, int page = 1)
        {
            var lop = await _context.LopHocs
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lop == null)
            {
                return NotFound();
            }

            int pageSize = 6;

            // Lấy các bài giảng liên kết với lớp học thông qua LopHocBaiGiang
            var baiGiangsQuery = _context.LopHocBaiGiangs
                .Where(lbg => lbg.LopHocId == id)
                .Include(lbg => lbg.BaiGiang)
                .Select(lbg => lbg.BaiGiang)
                .OrderByDescending(b => b.CreatedDate);

            var paginatedBaiGiangs = await PhanTrang<BaiGiang>.CreateAsync(baiGiangsQuery, page, pageSize);

            var baiTapsQuery = _context.BaiTapLopHocs
                .Where(bt => bt.LopHocId == id)
                .Include(bt => bt.BaiTap)
                .Select(bt => bt.BaiTap)
                .OrderByDescending(bt => bt.CreatedDate);

            var paginatedBaiTaps = await PhanTrang<BaiTap>.CreateAsync(baiTapsQuery, page, pageSize);

            var vm = new LopHocViewModel
            {
                Id = lop.Id,
                Name = lop.Name,
                Picture = lop.Picture,
                BaiGiangs = paginatedBaiGiangs,
                BaiTaps = paginatedBaiTaps
            };

            return View(vm);
        }
        // Controller: Học sinh bấm vào bài tập
        public IActionResult ChiTietBaiTap(int id)
        {
            var baiTap = _context.BaiTaps
                .AsNoTracking()
                .Include(b => b.NopBais)
                .Include(b => b.BaiTapLopHocs)
                .FirstOrDefault(b => b.Id == id);

            if (baiTap == null) return NotFound();

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var nopBai = baiTap.NopBais.FirstOrDefault(nb => nb.UsersId == userId);

            var viewModel = new ChiTietBaiTapViewModel
            {
                BaiTap = baiTap,
                NopBai = nopBai
            };
            ViewBag.LopId = baiTap.BaiTapLopHocs.FirstOrDefault()?.LopHocId;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> NopBai(int TestId, IFormFile Attachment, int lopId)
        {
            if (Attachment == null || Attachment.Length == 0)
                return BadRequest("Không có tệp đính kèm.");

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var existing = await _context.NopBais
                .FirstOrDefaultAsync(n => n.TestId == TestId && n.UsersId == userId);

            // ❌ Nếu đã chấm điểm thì không cho nộp lại
            if (existing != null && existing.Point.HasValue)
            {
                TempData["Error"] = "Bài tập đã được chấm điểm, bạn không thể nộp lại.";
                return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
            }

            // 🧾 Lưu file mới
            var fileName = $"{Guid.NewGuid()}_{Attachment.FileName}";
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Attachment.CopyToAsync(stream);
            }
            var fileUrl = "/uploads/" + fileName;

            // Nộp mới hoặc cập nhật nếu chưa chấm
            if (existing == null)
            {
                var nopBai = new NopBai
                {
                    TestId = TestId,
                    UsersId = userId,
                    SubmittedDate = DateTime.Now,
                    FileUrl = fileUrl
                };
                _context.NopBais.Add(nopBai);
            }
            else
            {
                existing.SubmittedDate = DateTime.Now;
                existing.FileUrl = fileUrl;
                existing.Point = null;
                existing.FeedBack = null;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Bài tập đã được nộp thành công.";

            return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
        }




    }
}
