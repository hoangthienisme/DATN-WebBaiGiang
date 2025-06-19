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

        public async Task<IActionResult> Courses(int page = 1)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var myCoursesQuery = _context.LopHocs
                .Where(l => l.IsActive && l.SinhVienLopHocs.Any(gv => gv.IdSv == userId))
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

            var baiTaps = await _context.BaiTapLopHocs
                .Where(bt => bt.LopHocId == id)
                .Include(bt => bt.BaiTap)
                .Select(bt => bt.BaiTap)
                .ToListAsync();

            var vm = new LopHocViewModel
            {
                Id = lop.Id,
                Name = lop.Name,
                Picture = lop.Picture,
                BaiGiangs = paginatedBaiGiangs,
                BaiTaps = baiTaps
            };

            return View("~/Views/Courses/DetailCourses.cshtml", vm);
        }
        // Controller: Học sinh bấm vào bài tập
        public IActionResult ChiTietBaiTap(int id)
        {
            var baiTap = _context.BaiTaps
                .Include(b => b.NopBais)
                .FirstOrDefault(b => b.Id == id);

            if (baiTap == null) return NotFound();

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var nopBai = baiTap.NopBais.FirstOrDefault(nb => nb.UsersId == userId);

            var viewModel = new ChiTietBaiTapViewModel
            {
                BaiTap = baiTap,
                NopBai = nopBai
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> NopBai(int TestId, IFormFile Attachment)
        {
            if (Attachment == null || Attachment.Length == 0)
                return BadRequest("Không có tệp đính kèm.");

            var fileName = $"{Guid.NewGuid()}_{Attachment.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Attachment.CopyToAsync(stream);
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var nopBai = new NopBai
            {
                TestId = TestId,
                UsersId = userId,
                FileUrl = "/uploads/" + fileName,
                SubmittedDate = DateTime.Now
            };

            _context.NopBais.Add(nopBai);
            await _context.SaveChangesAsync();

            return RedirectToAction("ChiTietBaiTap", new { id = TestId });
        }

    }
}
