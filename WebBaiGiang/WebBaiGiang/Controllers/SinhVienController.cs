using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<ThongBaoHub> _hubContext;
        public SinhVienController(WebBaiGiangContext context, IWebHostEnvironment env, IHubContext<ThongBaoHub> hubContext)
        {
            _context = context;
            _env = env;
            _hubContext = hubContext;
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

            if (existing != null && existing.Point.HasValue)
            {
                TempData["Error"] = "Bài tập đã được chấm điểm, bạn không thể nộp lại.";
                return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
            }

            // 📁 Lưu file
            var fileName = $"{Guid.NewGuid()}_{Attachment.FileName}";
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Attachment.CopyToAsync(stream);
            }
            var fileUrl = "/uploads/" + fileName;

            if (existing == null)
            {
                existing = new NopBai
                {
                    TestId = TestId,
                    UsersId = userId,
                    SubmittedDate = DateTime.Now,
                    FileUrl = fileUrl
                };
                _context.NopBais.Add(existing);
            }
            else
            {
                existing.SubmittedDate = DateTime.Now;
                existing.FileUrl = fileUrl;
                existing.Point = null;
                existing.FeedBack = null;
            }

            // 🚨 Thêm thông báo cho giảng viên
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.BaiTapLopHocs)
                    .ThenInclude(btlh => btlh.LopHoc)
                .FirstOrDefaultAsync(bt => bt.Id == TestId);

             var lop = baiTap?.BaiTapLopHocs.FirstOrDefault(btlh => btlh.LopHocId == lopId)?.LopHoc;
                      var giangVienId = await _context.GiangVienLopHocs
              .Where(gl => gl.IdClass == lopId && gl.IsActive)
              .Select(gl => gl.IdGv)
              .FirstOrDefaultAsync();

            if (giangVienId != 0)
            {
                var tb = new ThongBao
                {
                    NguoiNhanId = giangVienId,
                    NoiDung = $"Sinh viên đã nộp bài tập mới.",
                    LienKet = Url.Action("ChiTietBaiTapGV", "Courses", new { baiTapId = TestId }),
                    ThoiGian = DateTime.Now,
                    DaDoc = false
                };

                _context.ThongBaos.Add(tb);

                await _hubContext.Clients.Group($"user_{giangVienId}")
                    .SendAsync("NhanThongBao", new
                    {
                        tieuDe = "Bài tập mới được nộp",
                        link = tb.LienKet,
                        thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                    });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Bài tập đã được nộp thành công.";

            return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
        }

        // Controller (BaiGiangController.cs)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ChiTietBaiGiang(int id)
        {
            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.TaiNguyens)
                .Include(bg => bg.Chuongs)
                .Include(bg => bg.BinhLuans).ThenInclude(bl => bl.NguoiDung)
                .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiang == null)
                return NotFound();

            var viewModel = new ChiTietBaiGiangViewModel
            {
                Id = baiGiang.Id,
                Title = baiGiang.Title,
                Description = baiGiang.Description,
                CreatedDate = baiGiang.CreatedDate,
                TaiNguyens = baiGiang.TaiNguyens.ToList(),
                Chuongs = baiGiang.Chuongs.OrderBy(c => c.SortOrder).ToList(),
                BinhLuans = baiGiang.BinhLuans.OrderByDescending(b => b.NgayTao).ToList(),
                BaiGiang = baiGiang
            };

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoadBaiByChuong(int chuongId)
        {
            var bais = await _context.Bais
                .Where(b => b.ChuongId == chuongId)
                .Include(b => b.TaiNguyens)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();

            return PartialView("_BaiTrongChuong", bais); // ✅ Model đúng là IEnumerable<Bai>
        }




    }
}
