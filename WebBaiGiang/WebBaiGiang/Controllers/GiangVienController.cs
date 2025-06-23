using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

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

        // --- Courses (LopHoc) Management ---

        public async Task<IActionResult> Courses(int page = 1)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var myCoursesQuery = _context.LopHocs
                .Where(l => l.IsActive && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
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


        [HttpGet]
        public IActionResult CreateCourses()
        {
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList(); // For assigning a BaiGiang to a LopHoc

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourses(LopHoc lophoc, string DetailedDescription, IFormFile? Thumbnail) // Make IFormFile nullable for robustness
        {

            lophoc.Description = DetailedDescription;

            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                return Unauthorized();
            }
            ModelState.Remove(nameof(lophoc.GiangVienLopHocs));

            if (ModelState.IsValid)
            {
                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) // Ensure directory exists
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Thumbnail.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(stream);
                    }
                    lophoc.Picture = "/uploads/" + uniqueFileName;
                }

                // Explicitly set CreatedDate here to prevent default DateTime overflow if it's not set by default in DB
                lophoc.CreatedDate = DateTime.Now;
                lophoc.CreatedBy = giangVienId;
                lophoc.IsActive = true; // Set default for new course if not handled elsewhere

                _context.LopHocs.Add(lophoc);
                await _context.SaveChangesAsync(); // Save LopHoc to get its Id

                var gvlh = new GiangVienLopHoc
                {
                    IdClass = lophoc.Id,
                    IdGv = giangVienId,
                    AssignedDate = DateTime.Now, // <--- Ensure this DateTime is set
                    IsActive = true
                };
                _context.GiangVienLopHocs.Add(gvlh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lớp học đã được tạo thành công!";
                return RedirectToAction("Courses");
            }

            // If ModelState is not valid, re-populate ViewBags
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList();
            return View(lophoc); // Pass the lophoc back to the view to retain entered data
        }

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
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList(); 

            return View(lopHoc);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourses(LopHoc lophoc, string DetailedDescription, IFormFile? Thumbnail)
        {
            var existingLop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == lophoc.Id); // Use FindAsync for primary key lookup
            if (existingLop == null) return NotFound();

            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                return Unauthorized();
            }

            ModelState.Remove(nameof(lophoc.GiangVienLopHocs)); // Remove validation for navigation properties if they are not bound from form

            if (ModelState.IsValid)
            {
                existingLop.Name = lophoc.Name;
                existingLop.SubjectsId = lophoc.SubjectsId;
                existingLop.KhoaId = lophoc.KhoaId;
                //existingLop.BaiGiangId = lophoc.BaiGiangId;
                existingLop.Description = DetailedDescription;
                existingLop.UpdateDate = DateTime.Now; // <--- Ensure this DateTime is set for updates
                existingLop.UpdateBy = giangVienId;

                if (Thumbnail != null && Thumbnail.Length > 0)
                {
                    // Delete old thumbnail if exists
                    if (!string.IsNullOrEmpty(existingLop.Picture))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, existingLop.Picture.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Thumbnail.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Thumbnail.CopyToAsync(stream);
                    }
                    existingLop.Picture = "/uploads/" + uniqueFileName;
                }
                else if (string.IsNullOrEmpty(lophoc.Picture) && !string.IsNullOrEmpty(existingLop.Picture))
                {
                    // If no new thumbnail uploaded AND the client clears the old picture path (e.g., if you have a hidden input)
                    // Then delete the old file and clear the path.
                    // This is for cases where you want to explicitly remove a picture without replacing it.
                    var oldFilePath = Path.Combine(_env.WebRootPath, existingLop.Picture.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    existingLop.Picture = null;
                }


                _context.LopHocs.Update(existingLop);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lớp học đã được cập nhật thành công!";
                return RedirectToAction("Courses");
            }

            // If ModelState is not valid, re-populate ViewBags
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.Description = lophoc.Description; // Pass back description for Sticky form
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList();
            return View(lophoc);
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveCourse(int id) // ẩn lớp học
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

        // --- BaiGiang Management ---
        [HttpGet]
        public async Task<IActionResult> TaoBaiGiang()
        {
            var viewModel = new BaiGiangCreateViewModel();
            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                return RedirectToAction("Login", "Account");
            }

            viewModel.AvailableClasses = await _context.GiangVienLopHocs
                                                    .Where(glh => glh.IdGv == currentGiangVienId && glh.IdClassNavigation.IsActive == true)
                                                    .Select(glh => new SelectListItem
                                                    {
                                                        Value = glh.IdClass.ToString(),
                                                        Text = glh.IdClassNavigation.Name // Lấy tên lớp từ navigation property
                                                    })
                                                    .Distinct() // Đảm bảo không có lớp trùng lặp nếu có nhiều GiangVienLopHoc cho cùng một lớp
                                                    .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoBaiGiang(BaiGiangCreateViewModel model)
        {
            ModelState.Remove(nameof(model.AvailableClasses));

            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentGiangVienId = int.TryParse(currentGiangVienIdString, out var parsedId) ? parsedId : (int?)null;

            if (!ModelState.IsValid)
            {
                model.AvailableClasses = currentGiangVienId.HasValue
                    ? await _context.GiangVienLopHocs
                        .Where(glh => glh.IdGv == currentGiangVienId.Value && glh.IdClassNavigation.IsActive)
                        .Select(glh => new SelectListItem
                        {
                            Value = glh.IdClass.ToString(),
                            Text = glh.IdClassNavigation.Name
                        }).Distinct().ToListAsync()
                    : new List<SelectListItem>();

                return View(model);
            }

            // Tạo thư mục lưu file nếu chưa có
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
            Directory.CreateDirectory(imagePath);
            Directory.CreateDirectory(filePath);

            var baiGiang = new BaiGiang
            {
                Title = model.Title,
                Description = model.Description,
                CreatedDate = DateTime.Now,
                CreatedBy = currentGiangVienId,
                TaiNguyens = new List<TaiNguyen>(),
                Chuongs = new List<Chuong>()
            };

            // Lưu ảnh vào wwwroot/images/
            if (model.ImageFiles?.Any() == true)
            {
                foreach (var file in model.ImageFiles)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var savePath = Path.Combine(imagePath, uniqueFileName);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    baiGiang.TaiNguyens.Add(new TaiNguyen
                    {
                        Url = $"/images/{uniqueFileName}",
                        Loai = "image"
                    });
                }
            }
            // Lưu liên kết YouTube (nếu có)
            if (model.YoutubeLinks?.Any() == true)
            {
                foreach (var link in model.YoutubeLinks.Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    baiGiang.TaiNguyens.Add(new TaiNguyen
                    {
                        Url = link.Trim(),
                        Loai = "youtube"
                    });
                }
            }

            // Lưu tài liệu vào wwwroot/files/
            if (model.DocumentFiles?.Any() == true)
            {
                foreach (var file in model.DocumentFiles)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var savePath = Path.Combine(filePath, uniqueFileName);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    baiGiang.TaiNguyens.Add(new TaiNguyen
                    {
                        Url = $"/files/{uniqueFileName}",
                        Loai = "tailieu"
                    });
                }
            }

            // Xử lý chương và bài
            if (model.Chuongs?.Any() == true)
            {
                foreach (var chuongVm in model.Chuongs)
                {
                    var chuong = new Chuong
                    {
                        Title = chuongVm.Title,
                        SortOrder = chuongVm.SortOrder,
                        CreatedDate = DateTime.Now,
                        Bais = new List<Bai>()
                    };

                    if (chuongVm.Bais?.Any() == true)
                    {
                        foreach (var baiVm in chuongVm.Bais)
                        {
                            var bai = new Bai
                            {
                                Title = baiVm.Title,
                                Description = baiVm.Description,
                                VideoUrl = baiVm.VideoUrl,
                                SortOrder = baiVm.SortOrder,
                                CreatedDate = DateTime.Now,
                                TaiNguyens = new List<TaiNguyen>()
                            };

                            // Ảnh bài học
                            if (baiVm.ImageFiles?.Any() == true)
                            {
                                foreach (var file in baiVm.ImageFiles)
                                {
                                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                    var savePath = Path.Combine(imagePath, uniqueFileName);

                                    using var stream = new FileStream(savePath, FileMode.Create);
                                    await file.CopyToAsync(stream);

                                    bai.TaiNguyens.Add(new TaiNguyen
                                    {
                                        Url = $"/images/{uniqueFileName}",
                                        Loai = file.ContentType.StartsWith("video") ? "video" : "image"
                                    });
                                }
                            }

                            // Tài liệu bài học
                            if (baiVm.DocumentFiles?.Any() == true)
                            {
                                foreach (var file in baiVm.DocumentFiles)
                                {
                                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                    var savePath = Path.Combine(filePath, uniqueFileName);

                                    using var stream = new FileStream(savePath, FileMode.Create);
                                    await file.CopyToAsync(stream);

                                    bai.TaiNguyens.Add(new TaiNguyen
                                    {
                                        Url = $"/files/{uniqueFileName}",
                                        Loai = "tailieu"
                                    });
                                }
                            }

                            chuong.Bais.Add(bai);
                        }
                    }

                    baiGiang.Chuongs.Add(chuong);
                }
            }

            _context.BaiGiangs.Add(baiGiang);
            await _context.SaveChangesAsync();

            // Gán lớp học
            if (model.SelectedClassIds?.Any() == true)
            {
                foreach (var classId in model.SelectedClassIds)
                {
                    if (await _context.LopHocs.FindAsync(classId) is { } lopHoc)
                    {
                        _context.LopHocBaiGiangs.Add(new LopHocBaiGiang
                        {
                            LopHocId = classId,
                            BaiGiangId = baiGiang.Id,
                            AddedDate = DateTime.Now
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Tạo bài giảng thành công, file đã được lưu vào hệ thống!";
            return RedirectToAction("BaiGiang", "GiangVien");
        }








        public async Task<IActionResult> BaiGiang(int page = 1)
        {
            int pageSize = 6;
            var baigiang = _context.BaiGiangs
                .OrderByDescending(bg => bg.CreatedDate); // Ensure CreatedDate is always valid or nullable

            var paginatedCourses = await PhanTrang<BaiGiang>.CreateAsync(baigiang, page, pageSize);
            return View(paginatedCourses);
        }
        [HttpGet]
        public async Task<IActionResult> SuaBaiGiang(int? id)
        {
            if (id == null) return NotFound();

            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.Chuongs).ThenInclude(c => c.Bais)
                .Include(bg => bg.LopHocBaiGiangs)
                .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiang == null) return NotFound();

            var currentGiangVienId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (baiGiang.CreatedBy != currentGiangVienId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa bài giảng này.";
                return RedirectToAction("BaiGiang");
            }

            var taiNguyens = await _context.TaiNguyens
                .Where(t => t.BaiGiangId == baiGiang.Id)
                .ToListAsync();

            var viewModel = new BaiGiangEditViewModel
            {
                Id = baiGiang.Id,
                Title = baiGiang.Title,
                Description = baiGiang.Description,
                SelectedClassIds = baiGiang.LopHocBaiGiangs.Select(x => x.LopHocId).ToList(),

                ExistingImages = taiNguyens
    .Where(t => t.BaiId == null && t.Loai == "image")
    .Select(t => new TaiNguyenViewModel { Id = t.Id, Url = t.Url }).ToList(),

                ExistingDocuments = taiNguyens
    .Where(t => t.BaiId == null && t.Loai == "document")
    .Select(t => new TaiNguyenViewModel { Id = t.Id, Url = t.Url, Name = Path.GetFileName(t.Url) }).ToList(),

                ExistingYoutubeLinks = taiNguyens
    .Where(t => t.BaiId == null && t.Loai == "youtube")
    .Select(t => t.Url).ToList(),



                Chuongs = baiGiang.Chuongs.OrderBy(c => c.SortOrder).Select(c => new ChuongEditViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    SortOrder = c.SortOrder,
                    Bais = c.Bais.OrderBy(b => b.SortOrder).Select(b => new BaiEditViewModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Description = b.Description,
                        VideoUrl = b.VideoUrl,
                        SortOrder = b.SortOrder,
                        ExistingImageUrls = taiNguyens.Where(t => t.BaiId == b.Id && t.Loai == "image").Select(t => t.Url).ToList(),
                        ExistingDocumentUrls = taiNguyens.Where(t => t.BaiId == b.Id && t.Loai == "document").Select(t => t.Url).ToList(),
                    }).ToList()
                }).ToList()
            };

            viewModel.AvailableClasses = await _context.GiangVienLopHocs
                .Where(glh => glh.IdGv == currentGiangVienId && glh.IdClassNavigation.IsActive)
                .Select(glh => new SelectListItem
                {
                    Value = glh.IdClass.ToString(),
                    Text = glh.IdClassNavigation.Name
                }).Distinct().ToListAsync();

            return View("SuaBaiGiang", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaBaiGiang(BaiGiangEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return View(model);
            }

            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.Chuongs).ThenInclude(c => c.Bais)
                .Include(bg => bg.LopHocBaiGiangs)
                .FirstOrDefaultAsync(bg => bg.Id == model.Id);

            if (baiGiang == null) return NotFound();

            baiGiang.Title = model.Title;
            baiGiang.Description = model.Description;
            baiGiang.UpdateDate = DateTime.Now;
            //baiGiang.UpdateBy = User.Identity.Name;

            // Cập nhật lớp học
            _context.LopHocBaiGiangs.RemoveRange(baiGiang.LopHocBaiGiangs);
            baiGiang.LopHocBaiGiangs = model.SelectedClassIds.Select(classId => new LopHocBaiGiang
            {
                BaiGiangId = baiGiang.Id,
                LopHocId = classId
            }).ToList();

            // Cập nhật chương và bài học (tùy yêu cầu bạn có thể xóa hết rồi thêm lại hoặc so sánh cập nhật)
            _context.Chuongs.RemoveRange(baiGiang.Chuongs);
            baiGiang.Chuongs = model.Chuongs.Select((c, index) => new Chuong
            {
                Title = c.Title,
                SortOrder = c.SortOrder,
                Bais = c.Bais.Select((b, bIndex) => new Bai
                {
                    Title = b.Title,
                    Description = b.Description,
                    VideoUrl = b.VideoUrl,
                    SortOrder = b.SortOrder
                }).ToList()
            }).ToList();

            // Xử lý file mới (ảnh, tài liệu)
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            List<TaiNguyen> newTaiNguyens = new();

            if (model.ImageFiles != null)
            {
                foreach (var file in model.ImageFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    newTaiNguyens.Add(new TaiNguyen
                    {
                        BaiGiangId = baiGiang.Id,
                        Url = "/uploads/" + fileName,
                        Loai = "image"
                    });
                }
            }

            if (model.DocumentFiles != null)
            {
                foreach (var file in model.DocumentFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    newTaiNguyens.Add(new TaiNguyen
                    {
                        BaiGiangId = baiGiang.Id,
                        Url = "/uploads/" + fileName,
                        Loai = "document"
                    });
                }
            }

            if (model.YoutubeLinks != null)
            {
                foreach (var link in model.YoutubeLinks.Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    newTaiNguyens.Add(new TaiNguyen
                    {
                        BaiGiangId = baiGiang.Id,
                        Url = link.Trim(),
                        Loai = "youtube"
                    });
                }
            }

            _context.TaiNguyens.AddRange(newTaiNguyens);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật bài giảng thành công.";
            return RedirectToAction("BaiGiang");
        }


        private async Task<string> SaveFile(IFormFile file, string folderPath)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, folderPath);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folderPath}/{fileName}".Replace("\\", "/");
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

            // Delete associated files
            if (!string.IsNullOrEmpty(baiGiang.ContentUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, baiGiang.ContentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            foreach (var chuong in baiGiang.Chuongs)
            {
                foreach (var bai in chuong.Bais)
                {
                    if (!string.IsNullOrEmpty(bai.Document))
                    {
                        var filePath = Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }
            }

            _context.BaiGiangs.Remove(baiGiang);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài giảng thành công!";
            return RedirectToAction("BaiGiang");
        }
        public async Task<IActionResult> ChiTietBaiGiang(int id)
        {
            var baiGiang = _context.BaiGiangs
     .Include(bg => bg.Chuongs)
         .ThenInclude(c => c.Bais)
             .ThenInclude(b => b.TaiNguyens)
     .Include(bg => bg.TaiNguyens)
     .FirstOrDefault(bg => bg.Id == id);

            if (baiGiang == null)
                return NotFound();

            var viewModel = new BaiGiang
            {
                Id = baiGiang.Id,
                Title = baiGiang.Title,
                Description = baiGiang.Description,
                ContentUrl = baiGiang.ContentUrl,
                CreatedDate = baiGiang.CreatedDate,

                TaiNguyens = baiGiang.TaiNguyens,

                Chuongs = baiGiang.Chuongs.OrderBy(c => c.SortOrder).Select(c => new Chuong
                {
                    Id = c.Id,
                    Title = c.Title,
                    SortOrder = c.SortOrder,
                    Bais = c.Bais.OrderBy(b => b.SortOrder).Select(b => new Bai
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Description = b.Description,
                        VideoUrl = b.VideoUrl,
                        Document = b.Document,
                        SortOrder = b.SortOrder,
                        TaiNguyens = b.TaiNguyens
                    }).ToList()

                }).ToList()
            };
            return View(viewModel);
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