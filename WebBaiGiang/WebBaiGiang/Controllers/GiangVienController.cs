using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace WebBaiGiang.Controllers
{
    //[Authorize(Roles = "Admin,Teacher")]
    public class GiangVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _env;
        public GiangVienController(WebBaiGiangContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Courses()
        {

            var myCourses = _context.LopHocs
                .OrderByDescending(l => l.CreatedDate)
                .ToList();

            return View(myCourses);
        }

        [HttpGet]
        public IActionResult CreateCourses()
        {
            var subjects = _context.HocPhans.ToList();
            ViewBag.Subjects = subjects;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCourses(LopHoc lophoc, string DetailedDescription, IFormFile Thumbnail)
        {
            lophoc.Description = DetailedDescription;

            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh
                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Thumbnail.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(stream);
                    }

                    lophoc.Picture = "/uploads/" + uniqueFileName;
                }
                lophoc.CreatedDate = DateTime.Now;
                _context.LopHocs.Add(lophoc);
                await _context.SaveChangesAsync();

                return RedirectToAction("Courses");
            }

            return View("CreateCourses", lophoc);
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
