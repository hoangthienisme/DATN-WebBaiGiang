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

            var myCourses = _context.LopHocs
                .Where(l => l.IsActive == true && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
                .OrderByDescending(l => l.CreatedDate); // Ensure CreatedDate is always valid or nullable

            var paginatedCourses = await PhanTrang<LopHoc>.CreateAsync(myCourses, page, pageSize);
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
            ViewBag.BaiGiangs = _context.BaiGiangs.ToList(); // For assigning a BaiGiang to a LopHoc

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
                existingLop.BaiGiangId = lophoc.BaiGiangId;
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

            // Lấy danh sách các lớp học mà giảng viên hiện tại đang dạy
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

            // Lấy ID giảng viên hiện tại để gán vào CreatedBy (nếu có trong BaiGiang entity)
            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentGiangVienId = null;
            if (int.TryParse(currentGiangVienIdString, out int parsedId))
            {
                currentGiangVienId = parsedId;
            }

            if (!ModelState.IsValid)
            {
                // Nếu có lỗi, cần tải lại danh sách lớp học của giảng viên để hiển thị lại form
                if (currentGiangVienId.HasValue)
                {
                    model.AvailableClasses = await _context.GiangVienLopHocs
                                                        .Where(glh => glh.IdGv == currentGiangVienId.Value && glh.IdClassNavigation.IsActive == true)
                                                        .Select(glh => new SelectListItem
                                                        {
                                                            Value = glh.IdClass.ToString(),
                                                            Text = glh.IdClassNavigation.Name
                                                        })
                                                        .Distinct()
                                                        .ToListAsync();
                }
                else
                {
                    model.AvailableClasses = new List<SelectListItem>(); 
                }
                return View(model);
            }

            var baiGiang = new BaiGiang
            {
                Title = model.Title,
                Description = model.Description,
                ContentUrl = null, 
                CreatedDate = DateTime.Now,
                CreatedBy = currentGiangVienId 
            };
            if (model.Attachment != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "baigiang");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(stream);
                }
                baiGiang.ContentUrl = "/uploads/baigiang/" + fileName;
            }

  
            foreach (var chuongModel in model.Chuongs)
            {
                var chuong = new Chuong
                {
                    Title = chuongModel.Title,
                    SortOrder = chuongModel.SortOrder,
                    CreatedDate = DateTime.Now
                };

                foreach (var baiModel in chuongModel.Bais)
                {
                    var bai = new Bai
                    {
                        Title = baiModel.Title,
                        Description = baiModel.Description,
                        VideoUrl = baiModel.VideoUrl,
                        SortOrder = baiModel.SortOrder,
                        CreatedDate = DateTime.Now
                    };

                    if (baiModel.DocumentFile != null)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "bailearn");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var fileName = Guid.NewGuid() + Path.GetExtension(baiModel.DocumentFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await baiModel.DocumentFile.CopyToAsync(stream);
                        }
                        bai.Document = "/uploads/bailearn/" + fileName;
                    }
                    chuong.Bais.Add(bai);
                }
                baiGiang.Chuongs.Add(chuong);
            }

            _context.BaiGiangs.Add(baiGiang);
            await _context.SaveChangesAsync();
            foreach (var classId in model.SelectedClassIds)
            {
                var lopHoc = await _context.LopHocs.FindAsync(classId);
                if (lopHoc != null)
                {
                    lopHoc.BaiGiangId = baiGiang.Id;
                    lopHoc.UpdateDate = DateTime.Now; 
                    lopHoc.UpdateBy = currentGiangVienId; 
                    _context.LopHocs.Update(lopHoc);
                }
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bài giảng đã được tạo thành công và gán cho các lớp đã chọn!";
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
            if (id == null)
            {
                return NotFound();
            }
            var baiGiangToUpdate = await _context.BaiGiangs
                                                 .Include(bg => bg.Chuongs)
                                                     .ThenInclude(c => c.Bais)
                                                 .Include(bg => bg.LopHocs)
                                                 .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiangToUpdate == null)
            {
                return NotFound();
            }

            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Đảm bảo chỉ giảng viên tạo bài giảng mới được sửa
            if (baiGiangToUpdate.CreatedBy != currentGiangVienId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa bài giảng này.";
                return RedirectToAction("BaiGiang");
            }

            var viewModel = new BaiGiangEditViewModel
            {
                Id = baiGiangToUpdate.Id,
                Title = baiGiangToUpdate.Title,
                Description = baiGiangToUpdate.Description,
                ExistingAttachmentUrl = baiGiangToUpdate.ContentUrl,
                // Lấy danh sách các lớp đã gán cho bài giảng này
                SelectedClassIds = baiGiangToUpdate.LopHocs.Select(lh => lh.Id).ToList(),
                Chuongs = baiGiangToUpdate.Chuongs
                                  .OrderBy(c => c.SortOrder)
                                  .Select(c => new ChuongViewModel
                                  {
                                      Id = c.Id,
                                      Title = c.Title,
                                      SortOrder = c.SortOrder,
                                      Bais = c.Bais
                                              .OrderBy(b => b.SortOrder)
                                              .Select(b => new BaiViewModel
                                              {
                                                  Id = b.Id,
                                                  Title = b.Title,
                                                  Description = b.Description,
                                                  VideoUrl = b.VideoUrl,
                                                  ExistingDocumentUrl = b.Document,
                                                  SortOrder = b.SortOrder
                                              }).ToList()
                                  }).ToList()
            };

 
            // Lấy danh sách các lớp học mà giảng viên hiện tại đang dạy
            viewModel.AvailableClasses = await _context.GiangVienLopHocs
                                                    .Where(glh => glh.IdGv == currentGiangVienId && glh.IdClassNavigation.IsActive == true)
                                                    .Select(glh => new SelectListItem
                                                    {
                                                        Value = glh.IdClass.ToString(),
                                                        Text = glh.IdClassNavigation.Name // Lấy tên lớp từ navigation property
                                                    })
                                                    .Distinct() // Đảm bảo không có lớp trùng lặp nếu có nhiều GiangVienLopHoc cho cùng một lớp
                                                    .ToListAsync();

            return View("SuaBaiGiang", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaBaiGiang(int id, BaiGiangEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            ModelState.Remove(nameof(model.AvailableClasses));

            // Lấy ID giảng viên hiện tại
            var currentGiangVienIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentGiangVienIdString, out int currentGiangVienId))
            {
                return RedirectToAction("Login", "Account");
            }
            var baiGiangToUpdate = await _context.BaiGiangs
                                                 .Include(bg => bg.Chuongs)
                                                     .ThenInclude(c => c.Bais)
                                                 .Include(bg => bg.LopHocs)
                                                 .FirstOrDefaultAsync(bg => bg.Id == id);

            if (baiGiangToUpdate == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sửa
            if (baiGiangToUpdate.CreatedBy != currentGiangVienId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa bài giảng này.";
                return RedirectToAction("BaiGiang");
            }

            // Nếu có lỗi xác thực, populate lại AvailableClasses và trả về View
            if (!ModelState.IsValid)
            {
                model.AvailableClasses = await _context.GiangVienLopHocs
                                                        .Where(glh => glh.IdGv == currentGiangVienId && glh.IsActive == true)
                                                        .Select(glh => new SelectListItem
                                                        {
                                                            Value = glh.IdClass.ToString(),
                                                            Text = glh.IdClassNavigation.Name
                                                        })
                                                        .Distinct()
                                                        .ToListAsync();
                model.SelectedClassIds = model.SelectedClassIds ?? new List<int>();
                return View("EditBaiGiang", model);
            }

            // Cập nhật thông tin bài giảng chính
            baiGiangToUpdate.Title = model.Title;
            baiGiangToUpdate.Description = model.Description;
            baiGiangToUpdate.UpdateDate = DateTime.Now;
            baiGiangToUpdate.UpdateBy = currentGiangVienId;

            // Xử lý file đính kèm tổng thể
            if (model.Attachment != null)
            {
                if (!string.IsNullOrEmpty(baiGiangToUpdate.ContentUrl) && System.IO.File.Exists(Path.Combine(_env.WebRootPath, baiGiangToUpdate.ContentUrl.TrimStart('/'))))
                {
                    System.IO.File.Delete(Path.Combine(_env.WebRootPath, baiGiangToUpdate.ContentUrl.TrimStart('/')));
                }
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "baigiang");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(stream);
                }
                baiGiangToUpdate.ContentUrl = $"/uploads/baigiang/{fileName}";
            }
            else if (string.IsNullOrEmpty(model.ExistingAttachmentUrl) && !string.IsNullOrEmpty(baiGiangToUpdate.ContentUrl))
            {
                if (System.IO.File.Exists(Path.Combine(_env.WebRootPath, baiGiangToUpdate.ContentUrl.TrimStart('/'))))
                {
                    System.IO.File.Delete(Path.Combine(_env.WebRootPath, baiGiangToUpdate.ContentUrl.TrimStart('/')));
                }
                baiGiangToUpdate.ContentUrl = null;
            }
          
            var selectedClassIds = model.SelectedClassIds ?? new List<int>();
            var currentClassIds = baiGiangToUpdate.LopHocs
                                                  .Select(lh => lh.Id)
                                                  .ToList();
            var classesToRemove = baiGiangToUpdate.LopHocs
                                                  .Where(lh => !selectedClassIds.Contains(lh.Id))
                                                  .ToList();
            foreach (var lopHoc in classesToRemove)
            {
                
                baiGiangToUpdate.LopHocs.Remove(lopHoc);
            }
            var classIdsToAdd = selectedClassIds.Except(currentClassIds).ToList();
            if (classIdsToAdd.Any())
            {
                var classesToAdd = await _context.LopHocs
                                                 .Where(lh => classIdsToAdd.Contains(lh.Id))
                                                 .ToListAsync();
                foreach (var lopHoc in classesToAdd)
                {
                    
                    baiGiangToUpdate.LopHocs.Add(lopHoc);
                }
            }
            var currentChuongIdsInDb = baiGiangToUpdate.Chuongs.Select(c => c.Id).ToList();
            var currentBaiIdsInDb = baiGiangToUpdate.Chuongs.SelectMany(c => c.Bais).Select(b => b.Id).ToList();
            var baisToRemove = baiGiangToUpdate.Chuongs
                                               .SelectMany(c => c.Bais)
                                               .Where(bai => !model.Chuongs.Any(cm => cm.Bais.Any(bm => bm.Id == bai.Id && bai.Id != 0)))
                                               .ToList();

            foreach (var bai in baisToRemove)
            {
                if (!string.IsNullOrEmpty(bai.Document) && System.IO.File.Exists(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'))))
                {
                    System.IO.File.Delete(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/')));
                }
                _context.Bais.Remove(bai);
            }
            var chuongsToRemove = baiGiangToUpdate.Chuongs
                                                  .Where(chuong => !model.Chuongs.Any(cm => cm.Id == chuong.Id && chuong.Id != 0))
                                                  .ToList();

            foreach (var chuong in chuongsToRemove)
            {
                foreach (var bai in chuong.Bais.ToList())
                {
                    if (!string.IsNullOrEmpty(bai.Document) && System.IO.File.Exists(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'))))
                    {
                        System.IO.File.Delete(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/')));
                    }
                    _context.Bais.Remove(bai);
                }
                _context.Chuongs.Remove(chuong);
            }

            foreach (var chuongModel in model.Chuongs)
            {
                var chuong = baiGiangToUpdate.Chuongs.FirstOrDefault(c => c.Id == chuongModel.Id && chuongModel.Id != 0);

                if (chuong == null)
                {
                    chuong = new Chuong
                    {
                        Title = chuongModel.Title,
                        SortOrder = chuongModel.SortOrder,
                        CreatedDate = DateTime.Now,
                        BaiGiangId = baiGiangToUpdate.Id
                    };
                    baiGiangToUpdate.Chuongs.Add(chuong);
                }
                else
                {
                    chuong.Title = chuongModel.Title;
                    chuong.SortOrder = chuongModel.SortOrder;
                }

                foreach (var baiModel in chuongModel.Bais)
                {
                    var bai = chuong.Bais.FirstOrDefault(b => b.Id == baiModel.Id && baiModel.Id != 0);

                    if (bai == null)
                    {
                        bai = new Bai
                        {
                            Title = baiModel.Title,
                            Description = baiModel.Description,
                            VideoUrl = baiModel.VideoUrl,
                            SortOrder = baiModel.SortOrder,
                            CreatedDate = DateTime.Now,
                        };
                        chuong.Bais.Add(bai);
                    }
                    else
                    {
                        bai.Title = baiModel.Title;
                        bai.Description = baiModel.Description;
                        bai.VideoUrl = baiModel.VideoUrl;
                        bai.SortOrder = baiModel.SortOrder;
                    }

                    // Xử lý tài liệu đính kèm cho bài học
                    if (baiModel.DocumentFile != null)
                    {
                        if (!string.IsNullOrEmpty(bai.Document) && System.IO.File.Exists(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'))))
                        {
                            System.IO.File.Delete(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/')));
                        }
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "bailearn");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid() + Path.GetExtension(baiModel.DocumentFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await baiModel.DocumentFile.CopyToAsync(stream);
                        }
                        bai.Document = $"/uploads/bailearn/{fileName}";
                    }
                    else if (string.IsNullOrEmpty(baiModel.ExistingDocumentUrl) && !string.IsNullOrEmpty(bai.Document))
                    {
                        if (System.IO.File.Exists(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/'))))
                        {
                            System.IO.File.Delete(Path.Combine(_env.WebRootPath, bai.Document.TrimStart('/')));
                        }
                        bai.Document = null;
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bài giảng đã được cập nhật thành công!";
                return RedirectToAction(nameof(BaiGiang));
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật bài giảng. Vui lòng thử lại.";
                model.AvailableClasses = await _context.GiangVienLopHocs
                                                        .Where(glh => glh.IdGv == currentGiangVienId && glh.IsActive == true)
                                                        .Select(glh => new SelectListItem
                                                        {
                                                            Value = glh.IdClass.ToString(),
                                                            Text = glh.IdClassNavigation.Name
                                                        })
                                                        .Distinct()
                                                        .ToListAsync();
                model.SelectedClassIds = model.SelectedClassIds ?? new List<int>();
                return View("EditBaiGiang", model);
            }
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

        // --- Other actions (assuming they are complete and working) ---
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