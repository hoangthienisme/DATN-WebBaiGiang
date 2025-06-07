using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var myCourses = _context.LopHocs
                .Where(l => l.IsActive == true && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
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
        // sửa lớp học
        [HttpGet]
        public IActionResult EditCourses(int id)
        {
            var lopHoc = _context.LopHocs.Find(id);
            if (lopHoc == null)
            {
                return NotFound();
            }

            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.Description = lopHoc.Description;

            return View(lopHoc);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourses(LopHoc lophoc, string DetailedDescription, IFormFile Thumbnail)
        {
            var existingLop = _context.LopHocs.FirstOrDefault(x => x.Id == lophoc.Id);
            if (existingLop == null) return NotFound();

            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                existingLop.Name = lophoc.Name;
                existingLop.SubjectsId = lophoc.SubjectsId;
                existingLop.KhoaId = lophoc.KhoaId;
                existingLop.Description = DetailedDescription;
                existingLop.UpdateDate = DateTime.Now;
                existingLop.UpdateBy = giangVienId;

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

                    existingLop.Picture = "/uploads/" + uniqueFileName;
                }
                _context.LopHocs.Update(existingLop);
                await _context.SaveChangesAsync();

                return RedirectToAction("Courses");
            }

            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.Description = lophoc.Description;

            return View(lophoc);
        }

        // ẩn lớp học 
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveCourse(int id)
        {
            var course = await _context.LopHocs.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            course.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpGet]
        public IActionResult TaoBaiGiang()
        {
            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                // Xử lý khi không lấy được ID hợp lệ, ví dụ chuyển đến trang đăng nhập hoặc lỗi
                return RedirectToAction("Login", "Account");
            }

            ViewBag.LopList = _context.GiangVienLopHocs
                .Where(glh => glh.IdGv == currentGiangVienId)
                .Select(glh => new SelectListItem
                {
                    Value = glh.IdClass.ToString(),
                    Text = _context.LopHocs.FirstOrDefault(l => l.Id == glh.IdClass).Name
                })
                .ToList();

            var model = new BaiGiangCreateViewModel
            {
                ClassIds = new List<int>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TaoBaiGiang(BaiGiangCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("TaoBaiGiang", model);
            }

            // Lặp qua từng lớp được chọn
            foreach (var classId in model.ClassIds)
            {
                var baigiang = new BaiGiang
                {
                    ClassId = classId,
                    Title = model.Title,
                    Description = model.Description,
                    CreatedDate = DateTime.Now
                };

                // Xử lý file đính kèm bài giảng
                if (model.Attachment != null && model.Attachment.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }

                    baigiang.ContentUrl = "/uploads/" + fileName;
                }
                int chuongSort = 1;
                foreach (var chuongVm in model.Chuongs)
                {
                    var chuong = new Chuong
                    {
                        Title = chuongVm.Title,
                        SortOrder = chuongSort++,
                        CreatedDate = DateTime.Now
                    };

                    int baiSort = 1;
                    foreach (var baiVm in chuongVm.Bais)
                    {
                        var bai = new Bai
                        {
                            Title = baiVm.Title,
                            Description = baiVm.Description,
                            VideoUrl = baiVm.VideoUrl,
                            SortOrder = baiSort++,
                            CreatedDate = DateTime.Now
                        };

                        // Xử lý file tài liệu bài học (nếu có)
                        if (baiVm.DocumentFile != null && baiVm.DocumentFile.Length > 0)
                        {
                            var fileName = Guid.NewGuid() + Path.GetExtension(baiVm.DocumentFile.FileName);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await baiVm.DocumentFile.CopyToAsync(stream);
                            }

                            bai.Document = "/uploads/" + fileName;
                        }

                        chuong.Bais.Add(bai);
                    }

                    baigiang.Chuongs.Add(chuong);
                }
                _context.BaiGiangs.Add(baigiang);
            }
           
            await _context.SaveChangesAsync();
            return RedirectToAction("BaiGiang","GiangVien");
        }
        public async Task<IActionResult> BaiGiang(int page =1)
        {
            int pageSize = 6;
            
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }
            // Tìm các lớp học mà giáo viên này dạy
            var classIds = _context.GiangVienLopHocs
                .Where(gl => gl.IdGv == userId)
                .Select(gl => gl.IdClass)
                .ToList();

            //  Lấy bài giảng thuộc các lớp đó
            var baigiang = _context.BaiGiangs
                .Where(bg => classIds.Contains(bg.ClassId))
                .OrderByDescending(bg => bg.CreatedDate);
            var paginatedCourses = await PhanTrang<BaiGiang>.CreateAsync(baigiang, page, pageSize);
            return View(paginatedCourses);
        }
        // sửa bài giảng 
        [HttpGet]
        public IActionResult SuaBaiGiang(int id)
        {
            var baiGiang = _context.BaiGiangs
                .Include(bg => bg.Chuongs)
                    .ThenInclude(c => c.Bais)
                .FirstOrDefault(bg => bg.Id == id);

            if (baiGiang == null)
                return NotFound();

            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách lớp mà giảng viên dạy
            var availableClasses = _context.GiangVienLopHocs
                .Where(glh => glh.IdGv == currentGiangVienId)
                .Select(glh => new SelectListItem
                {
                    Value = glh.IdClass.ToString(),
                    Text = _context.LopHocs.FirstOrDefault(l => l.Id == glh.IdClass).Name
                })
                .ToList();

            var model = new BaiGiangEditViewModel
            {
                Id = baiGiang.Id,
                Title = baiGiang.Title,
                Description = baiGiang.Description,
                ClassIds = new List<int> { baiGiang.ClassId }, // nếu 1 lớp
                AvailableClasses = availableClasses,
                Chuongs = baiGiang.Chuongs
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new ChuongCreateViewModel
                    {
                        Title = c.Title,
                        SortOrder = c.SortOrder,
                        Bais = c.Bais
                            .OrderBy(b => b.SortOrder)
                            .Select(b => new BaiCreateViewModel
                            {
                                Title = b.Title,
                                Description = b.Description,
                                VideoUrl = b.VideoUrl,
                                SortOrder = b.SortOrder,
                                // Lưu ý: DocumentFile là upload mới, bạn có thể bổ sung 1 trường DocumentUrl nếu muốn hiển thị đường dẫn cũ trong View
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SuaBaiGiang(BaiGiangEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Phải load lại danh sách lớp để hiển thị dropdown
                var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
                {
                    model.AvailableClasses = _context.GiangVienLopHocs
                        .Where(glh => glh.IdGv == currentGiangVienId)
                        .Select(glh => new SelectListItem
                        {
                            Value = glh.IdClass.ToString(),
                            Text = _context.LopHocs.FirstOrDefault(l => l.Id == glh.IdClass).Name
                        }).ToList();
                }
                return View(model);
            }

            var baiGiang = _context.BaiGiangs
                .Include(bg => bg.Chuongs)
                    .ThenInclude(c => c.Bais)
                .FirstOrDefault(bg => bg.Id == model.Id);

            if (baiGiang == null)
                return NotFound();

            baiGiang.Title = model.Title;
            baiGiang.Description = model.Description;
            baiGiang.ClassId = model.ClassIds.FirstOrDefault();
            baiGiang.CreatedDate = DateTime.Now; // hoặc thêm trường ModifiedDate để đánh dấu chỉnh sửa

            // Xử lý file bài giảng
            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(stream);
                }
                baiGiang.ContentUrl = "/uploads/" + fileName;
            }

            // Xóa chương cũ và bài cũ
            _context.Chuongs.RemoveRange(baiGiang.Chuongs);
            baiGiang.Chuongs.Clear();

            int chuongSort = 1;
            foreach (var chuongVm in model.Chuongs)
            {
                var chuong = new Chuong
                {
                    Title = chuongVm.Title,
                    SortOrder = chuongSort++,
                    CreatedDate = DateTime.Now
                };

                int baiSort = 1;
                foreach (var baiVm in chuongVm.Bais)
                {
                    var bai = new Bai
                    {
                        Title = baiVm.Title,
                        Description = baiVm.Description,
                        VideoUrl = baiVm.VideoUrl,
                        SortOrder = baiSort++,
                        CreatedDate = DateTime.Now
                    };

                    if (baiVm.DocumentFile != null && baiVm.DocumentFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(baiVm.DocumentFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await baiVm.DocumentFile.CopyToAsync(stream);
                        }

                        bai.Document = "/uploads/" + fileName;
                    }
                    // Nếu bạn muốn giữ tài liệu cũ thì bạn cần thêm property DocumentUrl vào BaiCreateViewModel
                    // và gán cho bai.Document nếu không upload file mới
                    else
                    {
                        // Ví dụ:
                        // bai.Document = baiVm.DocumentUrl; // nếu bạn bổ sung property DocumentUrl trong VM
                    }

                    chuong.Bais.Add(bai);
                }

                baiGiang.Chuongs.Add(chuong);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("BaiGiang", "GiangVien");
        }

        [HttpPost]
        public async Task<IActionResult> XoaBaiGiang(int id)
        {
            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.Chuongs)
                .ThenInclude(ch => ch.Bais)
                .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiang == null)
            {
                return NotFound();
            }

            // Nếu có file đính kèm thì xóa luôn file vật lý ở đây nếu cần

            _context.BaiGiangs.Remove(baiGiang);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã xóa bài giảng thành công!";
            return RedirectToAction("BaiGiang");
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
