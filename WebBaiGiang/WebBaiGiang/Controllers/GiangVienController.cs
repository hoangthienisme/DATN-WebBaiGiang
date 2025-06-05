using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace WebBaiGiang.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class GiangVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _env;
        public GiangVienController(WebBaiGiangContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Courses(int page = 1)
        {
            int pageSize = 6;
            // Lấy userId (giáo viên id) từ claims, ví dụ bạn lưu userId dưới dạng ClaimTypes.NameIdentifier
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(); // Hoặc redirect login nếu chưa đăng nhập
            }


            var myCourses = _context.LopHocs
                .Where(l => l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
                .OrderByDescending(l => l.CreatedDate);
                
            var paginatedCourses = await PhanTrang<LopHoc>.CreateAsync(myCourses, page, pageSize);
            return View(paginatedCourses);
        }

        [HttpGet]
        public IActionResult CreateCourses()
        {
            var subjects = _context.HocPhans.ToList();
            var khoas = _context.Khoas.ToList();
            ViewBag.Subjects = subjects;
            ViewBag.Khoas = khoas;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCourses(LopHoc lophoc, string DetailedDescription, IFormFile Thumbnail)
        {
           
            lophoc.Description = DetailedDescription;
            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                return Unauthorized(); 
            }
            if (ModelState.IsValid)
            {
                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Thumbnail.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(stream);
                    }

                    lophoc.Picture = "/uploads/" + uniqueFileName;
                }

                lophoc.CreatedDate = DateTime.Now;
                lophoc.CreatedBy = giangVienId;
                _context.LopHocs.Add(lophoc);
                await _context.SaveChangesAsync();
                var gvlh = new GiangVienLopHoc
                {
                    IdClass = lophoc.Id,
                    IdGv = giangVienId,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                };
                _context.GiangVienLopHocs.Add(gvlh);
                await _context.SaveChangesAsync();

                return RedirectToAction("Courses");
            }
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();

            return View(); 
        }

        public IActionResult Stream()
        {
            return View();
        }
        public IActionResult Classwork()
        {
            return View();
        }
        public IActionResult DetailClasswork()
        {
            return View();
        }
    }
}
