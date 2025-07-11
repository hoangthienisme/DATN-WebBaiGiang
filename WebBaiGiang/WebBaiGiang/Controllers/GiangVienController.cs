﻿using Microsoft.AspNetCore.Mvc;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace WebBaiGiang.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class GiangVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<ThongBaoHub> _hubContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public GiangVienController(WebBaiGiangContext context, IWebHostEnvironment env, IHubContext<ThongBaoHub> hubContext, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _env = env;
            _hubContext = hubContext;
            _hostingEnvironment = hostingEnvironment;
        }

        // --- Courses (LopHoc) Management ---

        public async Task<IActionResult> Courses(string? search, int? subjectsId, int page = 1)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            // Query gốc
            var myCoursesQuery = _context.LopHocs
            .Include(l => l.Subjects) 
            .Where(l =>
                l.IsActive &&
                l.Subjects.IsActive && 
                l.GiangVienLopHocs.Any(gv => gv.IdGv == userId));


            // Lọc theo học phần nếu có
            if (subjectsId.HasValue)
            {
                myCoursesQuery = myCoursesQuery.Where(l => l.SubjectsId == subjectsId.Value);
            }

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

            // Gửi ViewBag ra view để giữ lại
            ViewBag.Search = search;
            ViewBag.SubjectsId = subjectsId;
            ViewBag.HocPhans = await _context.HocPhans
            .Where(h => h.IsActive)
            .ToListAsync();

            return View(paginatedCourses);
        }



        [HttpGet]
        public IActionResult CreateCourses()
        {
             ViewBag.Subjects = _context.HocPhans
           .Where(h => h.IsActive)
           .ToList();
        
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
            var hocPhan = await _context.HocPhans.FindAsync(lophoc.SubjectsId);
            if (hocPhan == null)
            {
                TempData["Error"] = "Không tìm thấy học phần đã chọn.";
                return RedirectToAction("Courses");
            }

            if (hocPhan.DepartmentId == null)
            {
                TempData["Error"] = "Học phần không liên kết với khoa nào.";
                return RedirectToAction("Courses");
            }

            lophoc.KhoaId = (int)hocPhan.DepartmentId;
            var giangVienIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(giangVienIdStr) || !int.TryParse(giangVienIdStr, out int giangVienId))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Courses");
            }
            var isDuplicate = await _context.LopHocs
                .AnyAsync(l => l.Name == lophoc.Name &&
                               l.GiangVienLopHocs.Any(gv => gv.IdGv == giangVienId) &&
                               l.IsActive);

            if (isDuplicate)
            {
                ModelState.AddModelError("Name", "Bạn đã có một lớp học với tên này.");

                ViewBag.Subjects = await _context.HocPhans.ToListAsync();
                ViewBag.Khoas = await _context.Khoas.ToListAsync();
                ViewBag.BaiGiangs = await _context.BaiGiangs.ToListAsync();

                ViewBag.TempName = lophoc.Name;
                ViewBag.TempDescription = DetailedDescription;
                ViewBag.TempThumbnailUrl = lophoc.Picture ?? "";
                ViewBag.TempSubjectsId = lophoc.SubjectsId;
                ViewBag.TempKhoaId = lophoc.KhoaId;
                ViewBag.TempSelectedBaiGiangIds = SelectedBaiGiangIds;

                return View("CreateCourses", lophoc);
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
        public IActionResult EditCourses(int id, string? OldPicture)
        {
            var lopHoc = _context.LopHocs.Find(id);
            if (lopHoc == null)
            {
                return NotFound();
            }

            ViewBag.Subjects = _context.HocPhans
          .Where(h => h.IsActive)
          .ToList();
            ViewBag.Description = lopHoc.Description;
            ViewBag.Picture = lopHoc.Picture;
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
                    var hocPhan = await _context.HocPhans.FindAsync(lophoc.SubjectsId);
                    if (hocPhan == null || hocPhan.DepartmentId == null)
                    {
                        TempData["Error"] = "Không thể cập nhật khoa từ học phần.";
                        return RedirectToAction("Courses");
                    }
                    existingLop.KhoaId = (int)hocPhan.DepartmentId;
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
                    // Gửi thông báo đến sinh viên trong các lớp
                    // Lấy danh sách sinh viên thuộc lớp học này
                    var sinhVienIds = await (from svlh in _context.SinhVienLopHocs
                                             join nd in _context.NguoiDungs
                                                 on svlh.IdSv equals nd.Id
                                             where svlh.IdClass == existingLop.Id
                                                   && nd.Role == "Student"
                                             select svlh.IdSv)
                         .Distinct()
                         .ToListAsync();

                    var dsThongBao = new List<ThongBao>();

                    foreach (var svId in sinhVienIds)
                    {
                        var tb = new ThongBao
                        {
                            NguoiNhanId = svId,
                            NoiDung = $"Lớp học \"{existingLop.Name}\" đã được cập nhật. Hãy kiểm tra lại thông tin!",
                            LienKet = Url.Action("DetailCourses", "Courses", new { id = existingLop.Id }),
                            Loai = LoaiThongBao.CapNhatLop, // Nếu có enum
                            ThoiGian = DateTime.Now,
                            DaDoc = false
                        };
                        dsThongBao.Add(tb);

                        // Gửi realtime
                        await _hubContext.Clients.Group($"user_{svId}")
                            .SendAsync("NhanThongBao", new
                            {
                                tieuDe = $"Lớp học \"{existingLop.Name}\" vừa cập nhật",
                                link = tb.LienKet,
                                thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                            });
                    }

                    _context.ThongBaos.AddRange(dsThongBao);
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
            ViewBag.Subjects = _context.HocPhans
          .Where(h => h.IsActive)
          .ToList();
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList();
            ViewBag.Description = lophoc.Description;
            ViewBag.Picture = existingLop.Picture;
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
        public async Task<IActionResult> ArchivedCourses(int page = 1, string? search = null, int? subjectsId = null)
        {
            int pageSize = 6;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var archivedQuery = _context.LopHocs
                .Where(l => !l.IsActive && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId));

            if (!string.IsNullOrEmpty(search))
            {
                archivedQuery = archivedQuery.Where(l => l.Name.Contains(search));
            }

            if (subjectsId.HasValue)
            {
                archivedQuery = archivedQuery.Where(l => l.SubjectsId == subjectsId.Value);
            }

            archivedQuery = archivedQuery.OrderByDescending(l => l.CreatedDate)
                .Select(l => new LopHoc
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    Picture = l.Picture,
                    CreatedDate = l.CreatedDate,
                    JoinCode = l.JoinCode

                });

            var paginated = await PhanTrang<LopHoc>.CreateAsync(archivedQuery, page, pageSize);

            // Truyền lại dữ liệu filter
            ViewBag.Search = search;
            ViewBag.SubjectsId = subjectsId;
            ViewBag.HocPhans = await _context.HocPhans.ToListAsync();

            return View("ArchivedCourses", paginated);
        }

        // hiện lại lớp học đã ẩn
        [HttpPost]
        public async Task<IActionResult> UnarchiveCourse(int id)
        {
            var course = await _context.LopHocs.FindAsync(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy lớp học.";
                return RedirectToAction("ArchivedCourses");
            }

            course.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã khôi phục lớp học.";
            return RedirectToAction("ArchivedCourses");
        }



        // --- BaiGiang Management ---
        [HttpGet]
        public async Task<IActionResult> TaoBaiGiang(string? returnUrl, int? classId)
        {
            var viewModel = new BaiGiangCreateViewModel();
            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                return RedirectToAction("Login", "Account");
            }

            viewModel.AvailableClasses = await _context.GiangVienLopHocs
                .Where(glh => glh.IdGv == currentGiangVienId && glh.IdClassNavigation.IsActive)
                .Select(glh => new SelectListItem
                {
                    Value = glh.IdClass.ToString(),
                    Text = glh.IdClassNavigation.Name
                })
                .Distinct()
                .ToListAsync();

            viewModel.SelectedClassIds = classId.HasValue ? new List<int> { classId.Value } : new List<int>();

            viewModel.AvailableHocPhans = await _context.HocPhans
                .Where(hp => hp.IsActive)
                .Select(hp => new SelectListItem
                {
                    Value = hp.Id.ToString(),
                    Text = hp.Name
                })
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

                // Lưu tạm YouTube link
                model.TempYoutubeLinks = model.YoutubeLinks?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(link => new TaiNguyen { Url = link, Loai = "youtube" }).ToList();

                // Lưu tạm ảnh (chỉ tên, không file)
                model.TempTaiNguyenImages = new List<TaiNguyen>();
                if (model.ImageFiles != null)
                {
                    foreach (var file in model.ImageFiles)
                    {
                        model.TempTaiNguyenImages.Add(new TaiNguyen
                        {
                            Url = file.FileName,
                            Loai = "image"
                        });
                    }
                }

                // Lưu tạm tài liệu (chỉ tên)
                model.TempTaiNguyenDocs = new List<TaiNguyen>();
                if (model.DocumentFiles != null)
                {
                    foreach (var file in model.DocumentFiles)
                    {
                        model.TempTaiNguyenDocs.Add(new TaiNguyen
                        {
                            Url = file.FileName,
                            Loai = "tailieu"
                        });
                    }
                }

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
                HocPhanId = model.HocPhanId,
                TaiNguyens = new List<TaiNguyen>(),
                Chuongs = new List<Chuong>()
            };

            // 🖼️ Ảnh bài giảng
            if (model.ImageFiles?.Any() == true)
            {
                foreach (var file in model.ImageFiles)
                {
                    try
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Lỗi khi upload file: {file.FileName} → {ex.Message}");
                        TempData["Error"] = $"Lỗi khi upload file: {file.FileName}";
                    }

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
                            // Video YouTube bài học
                            if (baiVm.YouTubeLinks?.Any() == true)
                            {
                                foreach (var link in baiVm.YouTubeLinks.Where(l => !string.IsNullOrWhiteSpace(l)))
                                {
                                    bai.TaiNguyens.Add(new TaiNguyen
                                    {
                                        Url = link.Trim(),
                                        Loai = "youtube"
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
            // Gửi thông báo đến sinh viên trong các lớp
            if (model.SelectedClassIds?.Any() == true)
            {
                var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ThongBaoHub>>();

                foreach (var classId in model.SelectedClassIds)
                {
                    // Lấy sinh viên thuộc lớp này
                                    var sinhVienIds = await _context.SinhVienLopHocs
                    .Where(sv => sv.IdClass == classId)
                    .Join(_context.NguoiDungs,
                          sv => sv.IdSv,
                          nd => nd.Id,
                          (sv, nd) => new { sv, nd })
                    .Where(x => x.nd.Role == "Student")
                    .Select(x => x.sv.IdSv)
                    .Distinct()
                    .ToListAsync();
                

                    var danhSachThongBao = sinhVienIds.Select(svId => new ThongBao
                    {
                        NguoiNhanId = svId,
                        NoiDung = $"Bài giảng mới \"{baiGiang.Title}\" đã được đăng.",
                        LienKet = Url.Action("ChiTietBaiGiang", "SinhVien", new { id = baiGiang.Id }),
                        Loai = LoaiThongBao.BaiGiangMoi,
                        ThoiGian = DateTime.Now
                    }).ToList();

                    _context.ThongBaos.AddRange(danhSachThongBao);

                    // 🔔 Gửi realtime từng sinh viên
                    foreach (var svId in sinhVienIds)
                    {
                        await hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                        {
                            tieuDe = $"Bài giảng mới: {baiGiang.Title}",
                            link = Url.Action("ChiTiet", "SinhVien", new { id = baiGiang.Id }),
                            thoiGian = DateTime.Now.ToString("HH:mm dd/MM")
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }


            TempData["Success"] = " Bài giảng đã được tạo và lưu thành công!";

            return returnUrl != null
           ? Redirect(returnUrl)
           : RedirectToAction("BaiGiang","GiangVien");
        }
        [HttpPost]
        [Route("GiangVien/UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            // CKEditor expects: { "url": "full_url" }
            return Json(new { url = fileUrl });
        }



        [HttpGet]
        public async Task<IActionResult> BaiGiang(string? search = null, int? hocPhanId = null)
        {
            // Lấy danh sách học phần cho dropdown
            var hocPhanList = await _context.HocPhans
     .Where(hp => hp.IsActive)
     .ToListAsync();
            ViewBag.HocPhanList = hocPhanList;

            ViewBag.HocPhanList = hocPhanList;
            ViewBag.Search = search;
            ViewBag.HocPhanId = hocPhanId;

            // Query gốc gồm cả BaiGiangs
            var query = _context.HocPhans
     .Where(hp => hp.IsActive)
     .Include(hp => hp.BaiGiangs)
     .AsQueryable();


            // Nếu lọc theo học phần cụ thể
            if (hocPhanId != null)
            {
                query = query.Where(hp => hp.Id == hocPhanId);
            }

            // Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.ToLower();
                query = query.Where(hp =>
                    hp.BaiGiangs.Any(bg =>
                        (bg.Title != null && bg.Title.ToLower().Contains(lowered)) ||
                        (bg.Description != null && bg.Description.ToLower().Contains(lowered))
                    )
                );
            }


            var hocPhans = await query.ToListAsync();
            return View(hocPhans);
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
        public async Task<IActionResult> XoaBaiGiang(int id, string? returnUrl, int? lopHocId)
        {
            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.Chuongs).ThenInclude(ch => ch.Bais)
                .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiang == null)
                return NotFound();

            // Xoá tài nguyên các bài trong chương
            var baiIds = baiGiang.Chuongs.SelectMany(c => c.Bais).Select(b => b.Id).ToList();
            var taiNguyensTheoBai = _context.TaiNguyens.Where(t => baiIds.Contains(t.BaiId ?? 0));
            _context.TaiNguyens.RemoveRange(taiNguyensTheoBai);

            // Xoá tài nguyên bài giảng cấp cao
            var taiNguyensTheoBaiGiang = _context.TaiNguyens.Where(t => t.BaiGiangId == id);
            _context.TaiNguyens.RemoveRange(taiNguyensTheoBaiGiang);

            // Xoá file các bài
            foreach (var chuong in baiGiang.Chuongs)
            {
                foreach (var bai in chuong.Bais)
                {
                    if (!string.IsNullOrEmpty(bai.Document))
                    {
                        var filePath = Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'));
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                }
            }

            // Xoá file chính của bài giảng
            if (!string.IsNullOrEmpty(baiGiang.ContentUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, baiGiang.ContentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            // Lấy lớp liên kết
            var lopIds = await _context.LopHocBaiGiangs
                .Where(lb => lb.BaiGiangId == baiGiang.Id)
                .Select(lb => lb.LopHocId)
                .ToListAsync();

            int lopGocId = lopHocId ?? lopIds.FirstOrDefault();

            //if (lopGocId == 0)
            //{
            //    return BadRequest("Không thể xác định lớp học liên quan.");
            //}
            // Ưu tiên ID truyền từ client

            // Lấy sinh viên trong lớp
            var sinhVienIds = await (from svlh in _context.SinhVienLopHocs
                                     join nd in _context.NguoiDungs
                                         on svlh.IdSv equals nd.Id
                                     where lopIds.Contains(svlh.IdClass) && nd.Role == "Student"
                                     select svlh.IdSv)
                           .Distinct()
                           .ToListAsync();


            _context.BaiGiangs.Remove(baiGiang);

            // Tạo danh sách thông báo
            var dsThongBao = new List<ThongBao>();
            var thoiGian = DateTime.Now;
            foreach (var svId in sinhVienIds)
            {
                dsThongBao.Add(new ThongBao
                {
                    NguoiNhanId = svId,
                    NoiDung = $"Bài giảng \"{baiGiang.Title}\" đã bị xóa bởi giảng viên.",
                    LienKet = Url.Action("DetailCourses", "Courses", new { id = lopGocId }) + "#lectureTab",
                    Loai = LoaiThongBao.XoaBaiGiang,
                    ThoiGian = thoiGian,
                    DaDoc = false
                });
            }

            _context.ThongBaos.AddRange(dsThongBao);
            await _context.SaveChangesAsync();

            // Gửi realtime
            foreach (var svId in sinhVienIds)
            {
                await _hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                {
                    tieuDe = $"Bài giảng \"{baiGiang.Title}\" đã bị xóa",
                    link = Url.Action("DetailCourses", "Courses", new { id = lopGocId }) + "#lectureTab",
                    thoiGian = thoiGian.ToString("HH:mm dd/MM")
                });
            }

            TempData["Success"] = "Đã xóa bài giảng thành công!";
            return !string.IsNullOrEmpty(returnUrl)
       ? Redirect(returnUrl)
       : (Request.Headers["Referer"].ToString().Contains("/GiangVien/BaiGiang")
           ? RedirectToAction("BaiGiang", "GiangVien")
           : RedirectToAction("DetailCourses", "Courses", new { id = lopGocId }));

        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChiTietBaiGiang(int id)
        {
            var baiGiang = _context.BaiGiangs
                .Include(bg => bg.TaiNguyens)
                .Include(bg => bg.Chuongs)
                .ThenInclude(c => c.Bais)
                .FirstOrDefault(bg => bg.Id == id);

            if (baiGiang == null)
                return NotFound();
            var hocPhans = _context.HocPhans.ToList();

            var viewModel = new ChiTietBaiGiangViewModel
            {
                Id = baiGiang.Id,
                Title = baiGiang.Title,
                Description = baiGiang.Description,
                CreatedDate = baiGiang.CreatedDate,
                TaiNguyens = baiGiang.TaiNguyens.ToList(),
                Chuongs = baiGiang.Chuongs.OrderBy(c => c.SortOrder).ToList(),
                BaiGiang = baiGiang,
                HocPhans = hocPhans,
                HocPhanId = baiGiang.HocPhanId
            };

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoadBaiByChuong(int chuongId)
        {
            var chuong = _context.Chuongs
                .Include(c => c.Bais)
                    .ThenInclude(b => b.TaiNguyens)
                .FirstOrDefault(c => c.Id == chuongId);

            if (chuong == null)
                return NotFound();

            return PartialView("_BaiTrongChuong", chuong.Bais.OrderBy(b => b.SortOrder).ToList());
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoadBinhLuan(int baiGiangId)
        {
            var binhLuans = _context.BinhLuans
                .Include(bl => bl.NguoiDung)
                .Where(bl => bl.BaiGiangId == baiGiangId)
                .OrderByDescending(bl => bl.NgayTao)
                .ToList();

            int currentUserId = 0;
            string currentUserRole = "";

            if (User.Identity.IsAuthenticated)
            {
                currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                currentUserRole = User.IsInRole("Teacher") ? "Teacher" : "Student";
            }

            var viewModel = new BinhLuanViewModel
            {
                BinhLuans = binhLuans,
                BaiGiangId = baiGiangId,
                CurrentUserId = currentUserId,
                CurrentUserRole = currentUserRole
            };

            return PartialView("_DanhSachBinhLuan", viewModel);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuan(int BaiGiangId, string NoiDung)
        {
            // Lấy ID người dùng hiện tại
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return RedirectToAction("DangNhap", "NguoiDung");

            // Kiểm tra nội dung bình luận
            if (string.IsNullOrWhiteSpace(NoiDung))
            {
                TempData["Error"] = "Bình luận không được để trống.";
                return RedirectToAction("ChiTietBaiGiang", new { id = BaiGiangId });
            }

            // Tạo bình luận mới
            var binhLuan = new BinhLuan
            {
                BaiGiangId = BaiGiangId,
                NguoiDungId = userId,
                NoiDung = NoiDung.Trim(),
                NgayTao = DateTime.Now
            };

            _context.BinhLuans.Add(binhLuan);

            // Lấy bài giảng kèm lớp học liên quan
            var baiGiang = await _context.BaiGiangs
                .Include(bg => bg.LopHocBaiGiangs)
                    .ThenInclude(lbg => lbg.LopHoc)
                .FirstOrDefaultAsync(bg => bg.Id == BaiGiangId);

            if (baiGiang != null)
            {
                // Lấy toàn bộ ID lớp chứa bài giảng
                var lopIds = baiGiang.LopHocBaiGiangs.Select(lbg => lbg.LopHocId).ToList();

                // Lấy sinh viên thuộc các lớp đó (trừ người đang bình luận)
                var sinhVienIds = await _context.SinhVienLopHocs
                    .Where(sv => lopIds.Contains(sv.IdClass) && sv.IdSv != userId)
                    .Select(sv => sv.IdSv)
                    .Distinct()
                    .ToListAsync();

                // Tạo và gửi thông báo
                var dsThongBao = new List<ThongBao>();
                var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ThongBaoHub>>();

                foreach (var svId in sinhVienIds)
                {
                    var tb = new ThongBao
                    {
                        NguoiNhanId = svId,
                        NoiDung = $"Bài giảng \"{baiGiang.Title}\" vừa có bình luận mới.",
                        LienKet = Url.Action("ChiTietBaiGiang", "SinhVien", new { id = BaiGiangId }),
                        Loai = LoaiThongBao.BinhLuanMoi,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    };

                    dsThongBao.Add(tb);

                    // Gửi realtime
                    await hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                    {
                        tieuDe = $"Bình luận mới: \"{baiGiang.Title}\"",
                        link = tb.LienKet,
                        thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                    });
                }

                _context.ThongBaos.AddRange(dsThongBao);
            }

            // Lưu vào DB
            await _context.SaveChangesAsync();

            var userRole = User.IsInRole("Student") ? "Student" :
               User.IsInRole("Teacher") ? "Teacher" : "";

            if (userRole == "Student")
            {
                string url = Url.Action("ChiTietBaiGiang", "SinhVien", new { id = BaiGiangId }) + "#comments";
                return Redirect(url);
            }
            else
            {
                string url = Url.Action("ChiTietBaiGiang", "GiangVien", new { id = BaiGiangId }) + "#comments";
                return Redirect(url);
            }
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
        public async Task<IActionResult> SaveAll(
      ChiTietBaiGiangViewModel model,
      List<IFormFile> ImageFiles,
      List<IFormFile> DocumentFiles,
      string YoutubeLinks)
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

                // Cập nhật thông tin cơ bản
                baiGiang.Title = model.Title ?? "";
                baiGiang.Description = model.Description ?? "";
                baiGiang.HocPhanId = model.HocPhanId;

                // Chỉ xóa ảnh cũ nếu đang upload ảnh mới
                if (ImageFiles != null && ImageFiles.Any())
                {
                    var oldImages = await _context.TaiNguyens
                        .Where(t => t.BaiGiangId == baiGiang.Id && t.Loai == "image")
                        .ToListAsync();
                    _context.TaiNguyens.RemoveRange(oldImages);
                }

                // Chỉ xóa tài liệu cũ nếu đang upload file tài liệu mới
                if (DocumentFiles != null && DocumentFiles.Any())
                {
                    var oldDocs = await _context.TaiNguyens
                        .Where(t => t.BaiGiangId == baiGiang.Id && t.Loai == "doc")
                        .ToListAsync();
                    _context.TaiNguyens.RemoveRange(oldDocs);
                }

                // Chỉ xóa YouTube cũ nếu có YouTube mới
                if (!string.IsNullOrWhiteSpace(YoutubeLinks))
                {
                    var oldYoutube = await _context.TaiNguyens
                        .Where(t => t.BaiGiangId == baiGiang.Id && t.Loai == "youtube")
                        .ToListAsync();
                    _context.TaiNguyens.RemoveRange(oldYoutube);
                }


                // Upload ảnh mới
                if (ImageFiles != null && ImageFiles.Any())
                {
                    var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "images");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var file in ImageFiles)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.TaiNguyens.Add(new TaiNguyen
                        {
                            BaiGiangId = baiGiang.Id,
                            Url = $"/uploads/images/{fileName}",
                            Loai = "image"
                        });
                    }
                }

                // Upload tài liệu mới
                if (DocumentFiles != null && DocumentFiles.Any())
                {
                    var docPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "docs");
                    if (!Directory.Exists(docPath))
                        Directory.CreateDirectory(docPath);

                    foreach (var file in DocumentFiles)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(docPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.TaiNguyens.Add(new TaiNguyen
                        {
                            BaiGiangId = baiGiang.Id,
                            Url = $"/uploads/docs/{fileName}",
                            Loai = "doc"
                        });
                    }
                }

                // Lưu link YouTube
                if (!string.IsNullOrWhiteSpace(YoutubeLinks))
                {
                    var youtubeUrls = YoutubeLinks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var url in youtubeUrls)
                    {
                        _context.TaiNguyens.Add(new TaiNguyen
                        {
                            BaiGiangId = baiGiang.Id,
                            Url = url,
                            Loai = "youtube"
                        });
                    }
                }

                // Cập nhật chương-bài
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

                // Gửi thông báo cho sinh viên
                var lopIds = await _context.LopHocBaiGiangs
                    .Where(lb => lb.BaiGiangId == baiGiang.Id)
                    .Select(lb => lb.LopHocId)
                    .ToListAsync();

                var sinhVienIds = await _context.SinhVienLopHocs
                    .Where(sv => lopIds.Contains(sv.IdClass))
                    .Join(_context.NguoiDungs,
                        sv => sv.IdSv,
                        nd => nd.Id,
                        (sv, nd) => new { sv, nd })
                    .Where(x => x.nd.Role == "Student")
                    .Select(x => x.sv.IdSv)
                    .Distinct()
                    .ToListAsync();

                var dsThongBao = new List<ThongBao>();
                var lienKet = Url.Action("ChiTietBaiGiang", "SinhVien", new { id = baiGiang.Id });

                foreach (var svId in sinhVienIds)
                {
                    var tb = new ThongBao
                    {
                        NguoiNhanId = svId,
                        NoiDung = $"Bài giảng \"{baiGiang.Title}\" đã được cập nhật.",
                        LienKet = lienKet,
                        Loai = LoaiThongBao.CapNhatBaiGiang,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    };
                    dsThongBao.Add(tb);

                    await _hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                    {
                        tieuDe = $"Bài giảng cập nhật: {baiGiang.Title}",
                        link = lienKet,
                        thoiGian = DateTime.Now.ToString("HH:mm dd/MM")
                    });
                }

                _context.ThongBaos.AddRange(dsThongBao);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemChuong(ThemChuongRequest model)
        {
            // Không dùng [FromBody] nữa
            try
            {
                var baiGiang = await _context.BaiGiangs.FindAsync(model.BaiGiangId);
                if (baiGiang == null)
                {
                    return Json(new { success = false, message = "Bài giảng không tồn tại" });
                }

                var maxSortOrder = await _context.Chuongs
                    .Where(c => c.BaiGiangId == model.BaiGiangId)
                    .MaxAsync(c => (int?)c.SortOrder) ?? 0;

                var chuong = new Chuong
                {
                    Title = model.Title,
                    SortOrder = maxSortOrder + 1,
                    BaiGiangId = model.BaiGiangId
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaChuong(int id)
        {
            try
            {
                var chuong = await _context.Chuongs
                    .Include(c => c.Bais)
                    .ThenInclude(b => b.TaiNguyens)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (chuong == null)
                {
                    return Json(new { success = false, message = "Chương không tồn tại" });
                }

                int baiGiangId = chuong.BaiGiangId; // Lưu lại để dùng sau khi xóa

                // Xóa tài nguyên
                var baiIds = chuong.Bais.Select(b => b.Id).ToList();
                var taiNguyens = _context.TaiNguyens.Where(t => baiIds.Contains(t.BaiId ?? 0));
                _context.TaiNguyens.RemoveRange(taiNguyens);

                // Xóa bài và chương
                _context.Bais.RemoveRange(chuong.Bais);
                _context.Chuongs.Remove(chuong);

                await _context.SaveChangesAsync();

                // Cập nhật lại SortOrder của các chương còn lại
                var chuongsConLai = await _context.Chuongs
                    .Where(c => c.BaiGiangId == baiGiangId)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();

                int newOrder = 1;
                foreach (var c in chuongsConLai)
                {
                    c.SortOrder = newOrder++;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = msg });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaChuong([FromBody] SuaChuongRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Json(new { success = false, message = "Tên chương không được để trống." });
            }

            var chuong = await _context.Chuongs.FindAsync(request.Id);
            if (chuong == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chương cần sửa." });
            }

            chuong.Title = request.Title.Trim();

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật chương thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
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
                            return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 5MB." });
                        }
                        var filePath = await SaveFile(file, "images");
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = filePath,
                            Loai = "image",
                            BaiGiangId = baiGiangId,
                            BaiId = bai.Id
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
                            return Json(new { success = false, message = "Kích thước tài liệu không được vượt quá 10MB." });
                        }
                        var filePath = await SaveFile(file, "files");
                        taiNguyens.Add(new TaiNguyen
                        {
                            Url = filePath,
                            Loai = "tailieu",
                            BaiGiangId = baiGiangId,
                            BaiId = bai.Id
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

        [HttpPost]
        public async Task<IActionResult> UploadAnhCKEditor(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest("No file uploaded.");

            var fileExtension = Path.GetExtension(upload.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", fileName);

            // Tạo thư mục nếu chưa tồn tại
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";

            var fullUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            return Json(new { uploaded = true, url = fullUrl });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaBai(IFormCollection form, List<IFormFile> images, List<IFormFile> docs)
        {
            int id = int.Parse(form["id"]);
            string title = form["title"];
            string description = form["description"];
            string youtubeLinks = form["youtubeLinks"];

            var bai = await _context.Bais.Include(b => b.TaiNguyens).FirstOrDefaultAsync(b => b.Id == id);
            if (bai == null)
                return Json(new { success = false, message = "Không tìm thấy bài học." });

            bai.Title = title;
            bai.Description = description;
            //bai.UpdateDate = DateTime.Now;
            //bai.UpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Xoá tài nguyên cũ nếu có file mới
            if (images.Any() || docs.Any() || !string.IsNullOrWhiteSpace(youtubeLinks))
            {
                var oldTaiNguyens = _context.TaiNguyens.Where(t => t.BaiId == bai.Id);
                _context.TaiNguyens.RemoveRange(oldTaiNguyens);
            }

            // Upload ảnh
            if (images != null && images.Any())
            {
                var imgFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/upload/images");
                if (!Directory.Exists(imgFolder))
                    Directory.CreateDirectory(imgFolder);

                foreach (var file in images)
                {
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(imgFolder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.TaiNguyens.Add(new TaiNguyen
                    {
                        BaiId = bai.Id,
                        Loai = "image",
                        Url = "/upload/images/" + fileName
                    });
                }
            }


            // Upload docs
            if (docs != null && docs.Any())
            {
                var docFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/upload/docs");
                if (!Directory.Exists(docFolder))
                    Directory.CreateDirectory(docFolder);

                foreach (var file in docs)
                {
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(docFolder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.TaiNguyens.Add(new TaiNguyen
                    {
                        BaiId = bai.Id,
                        Loai = "tailieu",
                        Url = "/upload/docs/" + fileName
                    });
                }
            }


            // YouTube links
            if (!string.IsNullOrWhiteSpace(youtubeLinks))
            {
                var links = youtubeLinks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var link in links)
                {
                    _context.TaiNguyens.Add(new TaiNguyen
                    {
                        BaiId = bai.Id,
                        Loai = "youtube",
                        Url = link
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

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

                int chuongId = bai.ChuongId;

                // Xóa tài nguyên liên quan
                var taiNguyens = _context.TaiNguyens.Where(t => t.BaiId == id);
                _context.TaiNguyens.RemoveRange(taiNguyens);

                _context.Bais.Remove(bai);
                await _context.SaveChangesAsync();

                //  Cập nhật lại SortOrder cho các bài còn lại trong chương
                var baisConLai = await _context.Bais
                    .Where(b => b.ChuongId == chuongId)
                    .OrderBy(b => b.SortOrder)
                    .ToListAsync();

                int newOrder = 1;
                foreach (var b in baisConLai)
                {
                    b.SortOrder = newOrder++;
                }

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