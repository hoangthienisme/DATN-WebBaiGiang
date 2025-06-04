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
       //     var user = _context.NguoiDungs
       //.FirstOrDefault(u => u.Email == email);


            return View(myCourses);
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
            Console.WriteLine($"KhoaId: {lophoc.KhoaId}, SubjectsId: {lophoc.SubjectsId}, Name: {lophoc.Name}");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}");
                    foreach (var e in error.Value.Errors)
                    {
                        Console.WriteLine($"  Error: {e.ErrorMessage}");
                    }
                }
            }
            lophoc.Description = DetailedDescription;

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
                _context.LopHocs.Add(lophoc);
                await _context.SaveChangesAsync();

                return RedirectToAction("Courses");
            }

            // Nếu lỗi -> load lại ViewBag
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
