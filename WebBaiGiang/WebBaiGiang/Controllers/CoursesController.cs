using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel;

namespace WebBaiGiang.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CoursesController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _env;
        public CoursesController(WebBaiGiangContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult BaiTap(int id)
        {
            ViewBag.LopId = id;
            // Gửi danh sách bài tập ra view
            var BaiTap = _context.BaiTaps.Where(x => x.ClassId == id).ToList();
            return View(BaiTap);
        }
        public IActionResult TaoBaiTap(int lopId)
        {
            ViewBag.LopId = lopId;
            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                // Xử lý khi không lấy được ID hợp lệ, ví dụ chuyển đến trang đăng nhập hoặc lỗi
                return RedirectToAction("Login", "Account");
            }

            var availableClasses = _context.GiangVienLopHocs
             .Where(glh => glh.IdGv == currentGiangVienId)
             .Select(glh => new SelectListItem
             {
                 Value = glh.IdClass.ToString(),
                 Text = _context.LopHocs.FirstOrDefault(l => l.Id == glh.IdClass).Name
             })
             .ToList();

            var model = new TaoBaiTapViewModel
            {
                ClassIds = new List<int>(),
                AvailableClasses = availableClasses,
                 LopIdGoc = lopId
            };
            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> TaoBaiTap(TaoBaiTapViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("TaoBaiTap", model);
            }


            foreach (var classId in model.ClassIds)
            {
                var baiTap = new BaiTap
                {
                    Title = model.Title,
                    Description = model.Description,
                    ClassId = classId,
                    DueDate = model.DueDate,
                    CreatedDate = DateTime.Now
                };

                if (model.Attachment != null && model.Attachment.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }

                    baiTap.ContentUrl = "/uploads/" + fileName;
                }

                _context.BaiTaps.Add(baiTap);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("BaiTap", "Courses", new { id = model.LopIdGoc });


        }

        [HttpGet]
        public async Task<IActionResult> DetailCourses(int id, int page = 1)
        {
            var lop = await _context.LopHocs
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lop == null)
            {
                return NotFound();
            }

            int pageSize = 6;
            var baiGiangsQuery = _context.BaiGiangs
                //.Where(b => b.ClassId == id)
                .OrderByDescending(b => b.CreatedDate);

            var paginatedBaiGiangs = await PhanTrang<BaiGiang>.CreateAsync(baiGiangsQuery, page, pageSize);

            var vm = new LopHocViewModel
            {
                Id = lop.Id,
                Name = lop.Name,
                Picture = lop.Picture,
                BaiGiangs = paginatedBaiGiangs
            };

            return View(vm);
        }

    }
}
