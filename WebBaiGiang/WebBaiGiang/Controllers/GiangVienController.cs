using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

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

        public async Task<IActionResult> Courses(string? search, int page = 1)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            // Query gốc
            var myCoursesQuery = _context.LopHocs
                .Where(l => l.IsActive && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId));

            // Nếu có từ khoá tìm kiếm thì lọc tiếp
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                myCoursesQuery = myCoursesQuery
                    .Where(l =>
                        l.Name.ToLower().Contains(keyword) ||
                        (!string.IsNullOrEmpty(l.Description) && l.Description.ToLower().Contains(keyword)));
            }

            // Chọn dữ liệu cần dùng
            var courseResult = myCoursesQuery
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

            // Gọi phân trang
            var paginatedCourses = await PhanTrang<LopHoc>.CreateAsync(courseResult, page, pageSize);

            // Gửi lại keyword ra view để giữ lại trong ô tìm
            ViewBag.Search = search;

            return View(paginatedCourses);
        }


        [HttpGet]
        public IActionResult CreateCourses()
        {
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList();

            // ĐỌC TempData rồi gán lại ViewBag để truyền vào View
            ViewBag.TempName = TempData["TempName"] as string ?? "";
            ViewBag.TempDescription = TempData["TempDescription"] as string ?? "";
            ViewBag.TempKhoaId = TempData["TempKhoaId"] as int?;
            ViewBag.TempSubjectsId = TempData["TempSubjectsId"] as int?;

            if (TempData["TempSelectedBaiGiangIds"] is string jsonIds)
            {
                var list = JsonSerializer.Deserialize<List<int>>(jsonIds);
                ViewBag.TempSelectedBaiGiangIds = list;
            }
            else
            {
                ViewBag.TempSelectedBaiGiangIds = new List<int>();
            }

            ViewBag.TempThumbnailUrl = TempData["TempThumbnailUrl"] as string ?? "";
            TempData.Keep("TempDescription");
            TempData.Keep("TempThumbnailUrl");
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> CreateCourses(LopHoc lophoc, string DetailedDescription, IFormFile? Thumbnail, List<int>? SelectedBaiGiangIds)

        {
            lophoc.Description = DetailedDescription;

            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Courses");
            }

            ModelState.Remove(nameof(lophoc.GiangVienLopHocs));

            if (ModelState.IsValid)
            {
                try
                {
                    if (Thumbnail != null && Thumbnail.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Thumbnail.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Thumbnail.CopyToAsync(stream);
                        }

                        lophoc.Picture = "/uploads/" + uniqueFileName;
                    }

                    lophoc.CreatedDate = DateTime.Now;
                    lophoc.CreatedBy = giangVienId;
                    lophoc.IsActive = true;

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
                    if (SelectedBaiGiangIds != null && SelectedBaiGiangIds.Any())
                    {
                        foreach (var baiGiangId in SelectedBaiGiangIds)
                        {
                            var exists = await _context.LopHocBaiGiangs
                                .AnyAsync(x => x.LopHocId == lophoc.Id && x.BaiGiangId == baiGiangId);
                            if (!exists)
                            {
                                _context.LopHocBaiGiangs.Add(new LopHocBaiGiang
                                {
                                    LopHocId = lophoc.Id,
                                    BaiGiangId = baiGiangId,
                                    AddedDate = DateTime.Now
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = " Lớp học đã được tạo thành công!";
                    return RedirectToAction("Courses");
                }
                catch
                {
                    TempData["Error"] = " Đã xảy ra lỗi khi tạo lớp học. Vui lòng thử lại.";
                    return RedirectToAction("Courses");
                }
            }

            TempData["Error"] = " Vui lòng kiểm tra lại thông tin lớp học.";
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList();
            return View(lophoc);
        }
        [HttpPost]
        public IActionResult LuuTamLopHoc(LopHoc lophoc, string DetailedDescription, List<int>? SelectedBaiGiangIds)
        {
            TempData["TempName"] = lophoc.Name?.Trim() ?? "";
            TempData["TempDescription"] = DetailedDescription ?? "";
            TempData["TempKhoaId"] = lophoc.KhoaId;
            TempData["TempSubjectsId"] = lophoc.SubjectsId;
            TempData["TempSelectedBaiGiangIds"] = SelectedBaiGiangIds ?? new List<int>();
            TempData["TempDescription"] = DetailedDescription;
            TempData["TempThumbnailUrl"] = lophoc.Picture; 


            if (SelectedBaiGiangIds != null)
                TempData["TempSelectedBaiGiangIds"] = JsonSerializer.Serialize(SelectedBaiGiangIds);

            return Ok();
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
        public async Task<IActionResult> EditCourses(LopHoc lophoc, string DetailedDescription, IFormFile? Thumbnail, string? OldPicture)
        {
            var existingLop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == lophoc.Id);
            if (existingLop == null)
            {
                TempData["Error"] = "Không tìm thấy lớp học cần chỉnh sửa.";
                return RedirectToAction("Courses");
            }

            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Courses");
            }

            ModelState.Remove(nameof(lophoc.GiangVienLopHocs));

            if (ModelState.IsValid)
            {
                try
                {
                    existingLop.Name = lophoc.Name;
                    existingLop.SubjectsId = lophoc.SubjectsId;
                    existingLop.KhoaId = lophoc.KhoaId;
                    existingLop.Description = DetailedDescription;
                    existingLop.UpdateDate = DateTime.Now;
                    existingLop.UpdateBy = giangVienId;

                    if (Thumbnail != null && Thumbnail.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingLop.Picture))
                        {
                            var oldFilePath = Path.Combine(_env.WebRootPath, existingLop.Picture.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);
                        }

                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Thumbnail.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Thumbnail.CopyToAsync(stream);
                        }

                        existingLop.Picture = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        // Giữ lại ảnh cũ nếu không chọn ảnh mới
                        existingLop.Picture = OldPicture;
                    }

                    _context.LopHocs.Update(existingLop);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Lớp học đã được cập nhật thành công!";
                    return RedirectToAction("Courses");
                }
                catch
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật lớp học.";
                    return RedirectToAction("Courses");
                }
            }

            TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            ViewBag.Subjects = _context.HocPhans.ToList();
            ViewBag.Khoas = _context.Khoas.ToList();
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList();
            ViewBag.Description = lophoc.Description;

            return View(lophoc);
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveCourse(int id)
        {
            var course = await _context.LopHocs.FindAsync(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy lớp học.";
                return RedirectToAction("Courses");
            }

            try
            {
                course.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lớp học đã được lưu trữ thành công.";
                return RedirectToAction("Courses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi lưu dữ liệu: " + ex.Message;
                return RedirectToAction("Courses");
            }
        }

        // hiện lớp học đã ẩn
        public async Task<IActionResult> ArchivedCourses(int page = 1)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var archivedQuery = _context.LopHocs
                .Where(l => !l.IsActive && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
                .OrderByDescending(l => l.CreatedDate)
                .Select(l => new LopHoc
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Picture = l.Picture,
                    CreatedDate = l.CreatedDate
                });

            var paginated = await PhanTrang<LopHoc>.CreateAsync(archivedQuery, page, pageSize);
            return View("ArchivedCourses", paginated);
        }
        // hiện lại lớp học đã ẩn
        [HttpPost]
        public JsonResult UnarchiveCourse(int id)
        {
            var course = _context.LopHocs.FirstOrDefault(x => x.Id == id);
            if (course == null)
                return Json(new { success = false });

            course.IsActive = true;
            _context.SaveChanges();

            return Json(new { success = true });
        }


        // --- BaiGiang Management ---
        [HttpGet]
        public async Task<IActionResult> TaoBaiGiang(string? returnUrl)
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
            ViewBag.ReturnUrl = returnUrl;
            return View(viewModel);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> TaoBaiGiang(BaiGiangCreateViewModel model, string? returnUrl)
        {
            ModelState.Remove(nameof(model.AvailableClasses));

            var currentGiangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentGiangVienIdStr, out int giangVienId))
            {
                TempData["Error"] = "Không xác định được giảng viên.";
                return RedirectToAction("BaiGiang", "GiangVien");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableClasses = await _context.GiangVienLopHocs
                    .Where(glh => glh.IdGv == giangVienId && glh.IdClassNavigation.IsActive)
                    .Select(glh => new SelectListItem
                    {
                        Value = glh.IdClass.ToString(),
                        Text = glh.IdClassNavigation.Name
                    }).Distinct().ToListAsync();

                return View(model);
            }

            var imagePath = Path.Combine(_env.WebRootPath, "images");
            var filePath = Path.Combine(_env.WebRootPath, "files");
            Directory.CreateDirectory(imagePath);
            Directory.CreateDirectory(filePath);

            var baiGiang = new BaiGiang
            {
                Title = model.Title,
                Description = model.Description,
                CreatedDate = DateTime.Now,
                CreatedBy = giangVienId,
                TaiNguyens = new List<TaiNguyen>(),
                Chuongs = new List<Chuong>()
            };

            // 🖼️ Ảnh bài giảng
            if (model.ImageFiles?.Any() == true)
            {
                foreach (var file in model.ImageFiles)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(imagePath, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    baiGiang.TaiNguyens.Add(new TaiNguyen
                    {
                        Url = $"/images/{fileName}",
                        Loai = "image"
                    });
                }
            }

            // 🔗 YouTube
            if (model.YoutubeLinks?.Any() == true)
            {
                foreach (var link in model.YoutubeLinks.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    baiGiang.TaiNguyens.Add(new TaiNguyen
                    {
                        Url = link.Trim(),
                        Loai = "youtube"
                    });
                }
            }

            // 📄 Tài liệu bài giảng
            if (model.DocumentFiles?.Any() == true)
            {
                foreach (var file in model.DocumentFiles)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(filePath, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    baiGiang.TaiNguyens.Add(new TaiNguyen
                    {
                        Url = $"/files/{fileName}",
                        Loai = "tailieu"
                    });
                }
            }

            // 📚 Chương & bài
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

                            // Ảnh hoặc video bài học
                            if (baiVm.ImageFiles?.Any() == true)
                            {
                                foreach (var file in baiVm.ImageFiles)
                                {
                                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                    var path = Path.Combine(imagePath, fileName);
                                    using var stream = new FileStream(path, FileMode.Create);
                                    await file.CopyToAsync(stream);

                                    bai.TaiNguyens.Add(new TaiNguyen
                                    {
                                        Url = $"/images/{fileName}",
                                        Loai = file.ContentType.StartsWith("video") ? "video" : "image"
                                    });
                                }
                            }

                            // Tài liệu bài học
                            if (baiVm.DocumentFiles?.Any() == true)
                            {
                                foreach (var file in baiVm.DocumentFiles)
                                {
                                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                    var path = Path.Combine(filePath, fileName);
                                    using var stream = new FileStream(path, FileMode.Create);
                                    await file.CopyToAsync(stream);

                                    bai.TaiNguyens.Add(new TaiNguyen
                                    {
                                        Url = $"/files/{fileName}",
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

            // ➕ Lưu bài giảng
            _context.BaiGiangs.Add(baiGiang);
            await _context.SaveChangesAsync();

            // Gán lớp học cho bài giảng
            if (model.SelectedClassIds?.Any() == true)
            {
                foreach (var classId in model.SelectedClassIds)
                {
                    _context.LopHocBaiGiangs.Add(new LopHocBaiGiang
                    {
                        LopHocId = classId,
                        BaiGiangId = baiGiang.Id,
                        AddedDate = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = " Bài giảng đã được tạo và lưu thành công!";

            return returnUrl != null
           ? Redirect(returnUrl)
           : RedirectToAction("BaiGiang","GiangVien");
        }


        public async Task<IActionResult> BaiGiang(int page = 1, string? search = null)
        {
            int pageSize = 6;

            var query = _context.BaiGiangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(bg =>
                    (bg.Title != null && bg.Title.Contains(search)) ||
                    (bg.Description != null && bg.Description.Contains(search)));
            }


            query = query.OrderByDescending(bg => bg.CreatedDate);

            ViewBag.Search = search;
            var paginatedLectures = await PhanTrang<BaiGiang>.CreateAsync(query, page, pageSize);
            return View(paginatedLectures);
        }

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty.");
            }
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_env.WebRootPath, folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/{folder}/{fileName}";
        }

        [HttpPost]
        public async Task<IActionResult> XoaBaiGiang(int id, string? returnUrl)
        {
            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.Chuongs)
                    .ThenInclude(ch => ch.Bais)
                .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiang == null)
            {
                return NotFound();
            }

            // Xoá tài nguyên liên quan đến các bài trong chương
            var baiIds = baiGiang.Chuongs.SelectMany(c => c.Bais).Select(b => b.Id).ToList();
            var taiNguyensTheoBai = _context.TaiNguyens.Where(t => baiIds.Contains(t.BaiId ?? 0));
            _context.TaiNguyens.RemoveRange(taiNguyensTheoBai);

            // Xoá tài nguyên cấp bài giảng (nếu có)
            var taiNguyensTheoBaiGiang = _context.TaiNguyens.Where(t => t.BaiGiangId == id);
            _context.TaiNguyens.RemoveRange(taiNguyensTheoBaiGiang);

            // Xoá file tài liệu trong các bài
            foreach (var chuong in baiGiang.Chuongs)
            {
                foreach (var bai in chuong.Bais)
                {
                    if (!string.IsNullOrEmpty(bai.Document))
                    {
                        var filePath = Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                }
            }

            // Xoá file chính của bài giảng (nếu có)
            if (!string.IsNullOrEmpty(baiGiang.ContentUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, baiGiang.ContentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.BaiGiangs.Remove(baiGiang);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa bài giảng thành công!";
            return returnUrl != null
                ? Redirect(returnUrl)
                : RedirectToAction("BaiGiang", "GiangVien");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChiTietBaiGiang(int id)
        {
            var baiGiang = _context.BaiGiangs
             .Include(bg => bg.TaiNguyens)
             .Include(bg => bg.Chuongs).ThenInclude(c => c.Bais).ThenInclude(b => b.TaiNguyens)
             .Include(bg => bg.BinhLuans).ThenInclude(bl => bl.NguoiDung)
             .FirstOrDefault(bg => bg.Id == id);

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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ThemBinhLuan(int BaiGiangId, string NoiDung)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var binhLuan = new BinhLuan
            {
                BaiGiangId = BaiGiangId,
                NguoiDungId = userId,
                NoiDung = NoiDung,
                NgayTao = DateTime.Now
            };

            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            return RedirectToAction("ChiTietBaiGiang", new { id = BaiGiangId });
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SuaBinhLuan(int id)
        {
            var bl = _context.BinhLuans.Include(x => x.NguoiDung).FirstOrDefault(x => x.Id == id);
            if (bl == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole == "SinhVien" && bl.NguoiDungId != currentUserId)
                return Forbid();

            return View(bl);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SuaBinhLuan(BinhLuan model)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(model.Id);
            if (binhLuan == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isGiangVien = User.IsInRole("GiangVien");

            if (binhLuan.NguoiDungId != currentUserId && !isGiangVien)
                return Forbid();

            binhLuan.NoiDung = model.NoiDung;
            //binhLuan.UpdateDate = DateTime.Now;

            _context.Update(binhLuan);
            await _context.SaveChangesAsync();

            return RedirectToAction("ChiTietBaiGiang", "GiangVien", new { id = binhLuan.BaiGiangId });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> XoaBinhLuan(int id)
        {
            var bl = await _context.BinhLuans.FindAsync(id);
            if (bl == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận cần xóa.";
                return RedirectToAction("ChiTietBaiGiang", "GiangVien");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdStr, out int currentUserId))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("ChiTietBaiGiang", "GiangVien", new { id = bl.BaiGiangId });
            }

            // Chỉ sinh viên mới bị giới hạn quyền
            if (userRole == "SinhVien" && bl.NguoiDungId != currentUserId)
            {
                TempData["Error"] = "Bạn không có quyền xóa bình luận này.";
                return RedirectToAction("ChiTietBaiGiang", "GiangVien", new { id = bl.BaiGiangId });
            }

            _context.BinhLuans.Remove(bl);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa bình luận thành công!";
            return RedirectToAction("ChiTietBaiGiang", "GiangVien", new { id = bl.BaiGiangId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAll(ChiTietBaiGiangViewModel model)
            {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var baiGiang = await _context.BaiGiangs
                    .Include(b => b.Chuongs).ThenInclude(c => c.Bais)
                    .FirstOrDefaultAsync(b => b.Id == model.Id);

                if (baiGiang == null)
                    return Json(new { success = false, message = "Bài giảng không tồn tại" });

                baiGiang.Title = model.Title ?? "";
                baiGiang.Description = model.Description ?? "";

                foreach (var chuong in model.Chuongs)
                {
                    var existingChuong = baiGiang.Chuongs.FirstOrDefault(c => c.Id == chuong.Id);
                    if (existingChuong != null)
                    {
                        existingChuong.Title = chuong.Title;

                        foreach (var bai in chuong.Bais)
                        {
                            var existingBai = existingChuong.Bais.FirstOrDefault(b => b.Id == bai.Id);
                            if (existingBai != null)
                            {
                                existingBai.Title = bai.Title;
                                existingBai.Description = bai.Description;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // POST: Thêm chương
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemChuong(int baiGiangId, string title)
        {
            try
            {
                var baiGiang = await _context.BaiGiangs.FindAsync(baiGiangId);
                if (baiGiang == null)
                {
                    return Json(new { success = false, message = "Bài giảng không tồn tại" });
                }

                var maxSortOrder = await _context.Chuongs
                    .Where(c => c.BaiGiangId == baiGiangId)
                    .MaxAsync(c => (int?)c.SortOrder) ?? 0;

                var chuong = new Chuong
                {
                    Title = title,
                    SortOrder = maxSortOrder + 1,
                    BaiGiangId = baiGiangId
                };

                _context.Chuongs.Add(chuong);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Xóa chương
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaChuong(int id)
        {
            try
            {
                var chuong = await _context.Chuongs
                    .Include(c => c.Bais)
                    .ThenInclude(b => b.TaiNguyens) // cần Include nếu có navigation
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (chuong == null)
                {
                    return Json(new { success = false, message = "Chương không tồn tại" });
                }

                // Xóa tài nguyên của từng bài trong chương
                var baiIds = chuong.Bais.Select(b => b.Id).ToList();
                var taiNguyens = _context.TaiNguyens.Where(t => baiIds.Contains(t.BaiId ?? 0));
                _context.TaiNguyens.RemoveRange(taiNguyens);

                _context.Bais.RemoveRange(chuong.Bais);
                _context.Chuongs.Remove(chuong);

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = msg });
            }
        }


        // POST: Thêm bài
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ThemBai(int baiId, int chuongId, int baiGiangId, string title, string description, IFormFile[] ImageFiles, string[] YoutubeLinks, IFormFile[] DocumentFiles)
        {
            try
            {


                // Kiểm tra chương
                var chuong = await _context.Chuongs.FindAsync(chuongId);
                if (chuong == null)
                {
                    return Json(new { success = false, message = "Chương không tồn tại" });
                }

                // Kiểm tra bài giảng
                var baiGiang = await _context.BaiGiangs.FindAsync(baiGiangId);
                if (baiGiang == null)
                {
                    return Json(new { success = false, message = "Bài giảng không tồn tại" });
                }

                // Kiểm tra quyền sở hữu
                var currentGiangVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (baiGiang.CreatedBy != currentGiangVienId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thêm bài cho bài giảng này." });
                }

                // Kiểm tra chương thuộc bài giảng
                if (chuong.BaiGiangId != baiGiangId)
                {
                    return Json(new { success = false, message = "Chương không thuộc bài giảng này" });
                }

                // Tạo bài mới
                var bai = new Bai
                {
                    ChuongId = chuongId,
                    Title = title,
                    Description = description,
                    SortOrder = (_context.Bais.Where(b => b.ChuongId == chuongId).Max(b => (int?)b.SortOrder) ?? 0) + 1
                };
                _context.Bais.Add(bai);
                await _context.SaveChangesAsync();

                // Xử lý tài nguyên
                var taiNguyens = new List<TaiNguyen>();

                // Upload ảnh
                if (ImageFiles?.Any(f => f.Length > 0) == true)
                {
                    foreach (var file in ImageFiles.Where(f => f.Length > 0))
                    {
                        if (file.Length > 5 * 1024 * 1024) // Giới hạn 5MB
                        {
                            TempData["Error"] = "Kích thước ảnh không được vượt quá 5MB.";
                            return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                        }
                        var filePath = await SaveFile(file, "images");
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = filePath,
                            Loai = "image",
                            BaiGiangId = baiGiangId,
                            BaiId = bai.Id // Sửa từ baiId thành bai.Id
                        });
                    }
                }

                // Upload tài liệu
                if (DocumentFiles?.Any(f => f.Length > 0) == true)
                {
                    foreach (var file in DocumentFiles.Where(f => f.Length > 0))
                    {
                        if (file.Length > 10 * 1024 * 1024) // Giới hạn 10MB
                        {
                            TempData["Error"] = "Kích thước tài liệu không được vượt quá 10MB.";
                            return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                        }
                        var filePath = await SaveFile(file, "files");
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = filePath,
                            Loai = "tailieu",
                            BaiGiangId = baiGiangId,
                            BaiId = bai.Id // Sửa từ baiId thành bai.Id
                        });
                    }
                }

                // Thêm link YouTube
                if (YoutubeLinks?.Any(l => !string.IsNullOrWhiteSpace(l)) == true)
                {
                    foreach (var link in YoutubeLinks.Where(l => !string.IsNullOrWhiteSpace(l)))
                    {
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = link.Trim(),
                            Loai = "youtube",
                            BaiGiangId = baiGiangId,
                            BaiId = bai.Id // Sửa từ baiId thành bai.Id
                        });
                    }
                }

                if (taiNguyens.Any())
                {
                    _context.TaiNguyens.AddRange(taiNguyens);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm bài." });
            }
        }

        // POST: Xóa bài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaBai(int id)
        {
            try
            {
                var bai = await _context.Bais.FindAsync(id);
                if (bai == null)
                {
                    return Json(new { success = false, message = "Bài không tồn tại" });
                }

                // Xóa các tài nguyên liên quan
                var taiNguyens = _context.TaiNguyens.Where(t => t.BaiId == id);
                _context.TaiNguyens.RemoveRange(taiNguyens);

                _context.Bais.Remove(bai);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = msg });
            }
        }

        // Thêm tài nguyên
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ThemTaiNguyen(int? baiId, int baiGiangId, IFormFile[] ImageFiles, IFormFile[] DocumentFiles, string[] YoutubeLinks)
        {
            try
            {
                var baiGiang = await _context.BaiGiangs.FindAsync(baiGiangId);
                if (baiGiang == null)
                {
                    TempData["Error"] = "Bài giảng không tồn tại";
                    return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                }

                if (baiId.HasValue)
                {
                                var bai = await _context.Bais
                     .Include(b => b.Chuong)
                     .FirstOrDefaultAsync(b => b.Id == baiId);
                    if (bai == null)
                    {
                        TempData["Error"] = "Bài không tồn tại";
                        return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                    }
                    if (bai.Chuong?.BaiGiangId != baiGiangId)
                    {
                        TempData["Error"] = "Bài không thuộc bài giảng này";
                        return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                    }
                }

                var currentGiangVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (baiGiang.CreatedBy != currentGiangVienId)
                {
                    TempData["Error"] = "Bạn không có quyền thêm tài nguyên cho bài giảng này.";
                    return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                }

                var taiNguyens = new List<TaiNguyen>();

                // Upload ảnh
                if (ImageFiles?.Any(f => f.Length > 0) == true)
                {
                    foreach (var file in ImageFiles.Where(f => f.Length > 0))
                    {
                        if (file.Length > 5 * 1024 * 1024) // Giới hạn 5MB
                        {
                            TempData["Error"] = "Kích thước ảnh không được vượt quá 5MB.";
                            return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                        }
                        var filePath = await SaveFile(file, "images");
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = filePath,
                            Loai = "image",
                            BaiGiangId = baiGiangId,
                            BaiId = baiId
                        });
                    }
                }

                // Upload tài liệu
                if (DocumentFiles?.Any(f => f.Length > 0) == true)
                {
                    foreach (var file in DocumentFiles.Where(f => f.Length > 0))
                    {
                        if (file.Length > 10 * 1024 * 1024) // Giới hạn 10MB
                        {
                            TempData["Error"] = "Kích thước tài liệu không được vượt quá 10MB.";
                            return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                        }
                        var filePath = await SaveFile(file, "files");
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = filePath,
                            Loai = "tailieu",
                            BaiGiangId = baiGiangId,
                            BaiId = baiId
                        });
                    }
                }

                // Thêm link YouTube
                if (YoutubeLinks?.Any(l => !string.IsNullOrWhiteSpace(l)) == true)
                {
                    foreach (var link in YoutubeLinks.Where(l => !string.IsNullOrWhiteSpace(l)))
                    {
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = link.Trim(),
                            Loai = "youtube",
                            BaiGiangId = baiGiangId,
                            BaiId = baiId
                        });
                    }
                }

                if (taiNguyens.Any())
                {
                    _context.TaiNguyens.AddRange(taiNguyens);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm tài nguyên thành công!";
                }
                else
                {
                    TempData["Error"] = "Không có tài nguyên nào được thêm.";
                }

                return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
            }
            catch 
            {
             
                TempData["Error"] = "Có lỗi xảy ra khi thêm tài nguyên.";
                return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
            }
        }

        // Xóa tài nguyên
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> XoaTaiNguyen(int id, int baiGiangId)
        {
            try
            {
                var taiNguyen = await _context.TaiNguyens.FindAsync(id);
                if (taiNguyen == null)
                {
                    TempData["Error"] = "Tài nguyên không tồn tại";
                    return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                }

                var baiGiang = await _context.BaiGiangs.FindAsync(baiGiangId);
                if (baiGiang == null)
                {
                    TempData["Error"] = "Bài giảng không tồn tại";
                    return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                }

                var currentGiangVienId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (baiGiang.CreatedBy != currentGiangVienId)
                {
                    TempData["Error"] = "Bạn không có quyền xóa tài nguyên này.";
                    return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
                }

                if (taiNguyen.Loai != "youtube")
                {
                    var filePath = Path.Combine(_env.WebRootPath, taiNguyen.Url.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.TaiNguyens.Remove(taiNguyen);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tài nguyên thành công!";
                return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
            }
            catch 
            {
             
                TempData["Error"] = "Có lỗi xảy ra khi xóa tài nguyên.";
                return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
            }
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