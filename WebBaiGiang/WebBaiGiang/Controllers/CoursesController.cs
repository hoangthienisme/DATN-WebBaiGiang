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
            ViewBag.LopId = _context.BaiTapLopHocs;

            var baiTaps = _context.BaiTapLopHocs
                .Where(bt => bt.LopHocId == id)
                .Include(bt => bt.BaiTap) 
                .Select(bt => bt.BaiTap)
                .ToList();

            return View(baiTaps);
        }

        // GET: Tạo bài tập
        [HttpGet]
        public IActionResult TaoBaiTap(int lopId)
        {
            var model = new BaiTapViewModel
            {
                LopIdGoc = lopId,
                AvailableClasses = _context.LopHocs
                    .Select(l => new SelectListItem { Text = l.Name, Value = l.Id.ToString() })
                    .ToList()
            };

            return View(model);
        }


        // POST: Lưu bài tập
        [HttpPost]

        public async Task<IActionResult> TaoBaiTap(BaiTapViewModel model)
        {
            // Validate model và kiểm tra ít nhất một lớp được chọn
            if (!ModelState.IsValid || model.ClassIds == null || !model.ClassIds.Any())
            {
                // Load lại danh sách lớp học nếu form bị lỗi
                model.AvailableClasses = _context.LopHocs
                 .Where(l => l.IsActive)
                 .Select(l => new SelectListItem
                 {
                     Value = l.Id.ToString(),
                     Text = l.Name
                 }).ToList();

                model.ClassIds = new List<int> { model.LopIdGoc }; // mặc định chọn lớp gốc nếu có


                ModelState.AddModelError("", "Vui lòng chọn ít nhất một lớp học.");
                return View(model);
            }

            // Xử lý file nếu có
            string? fileUrl = null;
            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(stream);
                }

                fileUrl = "/uploads/" + uniqueFileName;
            }

            model.LopIdGoc = model.ClassIds.FirstOrDefault();

            var baiTap = new BaiTap
            {
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                //MaxScore = model.MaxScore,
                CreatedDate = DateTime.Now,
                IsActive = true,
                BaiTapLopHocs = model.ClassIds.Select(id => new BaiTapLopHoc
                {
                    LopHocId = id,
                    NgayGiao = DateTime.Now
                }).ToList()
            };

            _context.BaiTaps.Add(baiTap);
            await _context.SaveChangesAsync();

            return RedirectToAction("BaiTap", "Courses", new { id = model.LopIdGoc });



        }

        [HttpGet]
        public IActionResult SuaBaiTap(int id)
        {
            var baiTap = _context.BaiTaps
                .Include(bt => bt.BaiTapLopHocs)
                .FirstOrDefault(x => x.Id == id);

            if (baiTap == null) return NotFound();

            var model = new BaiTapViewModel
            {
                Title = baiTap.Title,
                Description = baiTap.Description,
                DueDate = baiTap.DueDate,
                ContentUrl = baiTap.ContentUrl,
                ClassIds = baiTap.BaiTapLopHocs?.Select(x => x.BaiTapId).ToList() ?? new(),
                AvailableClasses = GetAvailableClasses()
            };

            ViewBag.Id = baiTap.Id;
            return View( model); 
        }

        [HttpPost]
        public async Task<IActionResult> SuaBaiTap(int id, BaiTapViewModel model)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.BaiTapLopHocs)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (baiTap == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.AvailableClasses = GetAvailableClasses();
                ViewBag.Id = id;
                return View( model);
            }

            baiTap.Title = model.Title;
            baiTap.Description = model.Description;
            baiTap.DueDate = model.DueDate;
            baiTap.ContentUrl = model.ContentUrl;

            if (model.Attachment != null)
            {
                var fileName = Path.GetFileName(model.Attachment.FileName);
                var path = Path.Combine("wwwroot/uploads", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await model.Attachment.CopyToAsync(stream);
                baiTap.ContentUrl = "/uploads/" + fileName;
            }

            // Xóa lớp cũ
            _context.BaiTapLopHocs.RemoveRange(baiTap.BaiTapLopHocs);

            foreach (var classId in model.ClassIds)
            {
                _context.BaiTapLopHocs.Add(new BaiTapLopHoc
                {
                    BaiTapId = baiTap.Id,
                    LopHocId = classId,
                    NgayGiao = DateTime.Now 
                });
            }


            await _context.SaveChangesAsync();
            return RedirectToAction("BaiTap", "Courses", new { id = model.LopIdGoc });
        }
        [HttpPost]
        public async Task<IActionResult> XoaBaiTap(int id)
        {
            var baiTap = await _context.BaiTaps.FindAsync(id);
            if (baiTap == null) return NotFound();

            var baiTapLops = _context.BaiTapLopHocs.Where(x => x.BaiTapId == id);
            _context.BaiTapLopHocs.RemoveRange(baiTapLops);
            _context.BaiTaps.Remove(baiTap);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetAvailableClasses()
        {
            return _context.LopHocs.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.Name
            }).ToList();
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
        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserToClassViewModel model)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng");
                return View(model);
            }

            // Kiểm tra đã có trong lớp chưa
            var exists = await _context.SinhVienLopHocs
                .AnyAsync(x => x.IdClass == model.ClassId && x.IdSv == user.Id);

            if (!exists)
            {
                var svLop = new SinhVienLopHoc
                {
                    IdClass = model.ClassId,
                    IdSv = user.Id,
                    JoinDate = DateTime.Now,
                    IsActive = true
                };

                _context.SinhVienLopHocs.Add(svLop);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ChiTietLop", new { id = model.ClassId });
        }



    }
}
