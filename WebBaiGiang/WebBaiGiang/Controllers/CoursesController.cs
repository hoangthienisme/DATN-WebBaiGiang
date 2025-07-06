using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.IO;
using NuGet.Packaging;
using OfficeOpenXml;
namespace WebBaiGiang.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CoursesController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IHubContext<ThongBaoHub> _hubContext;

        public CoursesController(WebBaiGiangContext context, IWebHostEnvironment env, IEmailService emailService, IHubContext<ThongBaoHub> hubContext)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        // Hiển thị danh sách bài tập theo lớp
        public IActionResult BaiTap(int id)
        {
            ViewBag.LopId = id;

            var baiTaps = _context.BaiTapLopHocs
                .Where(bt => bt.LopHocId == id)
                .Include(bt => bt.BaiTap)
                .OrderByDescending(bt => bt.BaiTap.CreatedDate)
                .Select(bt => bt.BaiTap)
                .ToList();

            return View(baiTaps);
        }

        // GET: Tạo bài tập
        [HttpGet]
        public IActionResult TaoBaiTap(int? lopId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var availableClasses = _context.LopHocs
                .Where(l => l.IsActive && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
                .Select(l => new SelectListItem
                {
                    Text = l.Name,
                    Value = l.Id.ToString()
                })
                .ToList();

            var lopIdGoc = lopId ?? (availableClasses.Any() ? int.Parse(availableClasses.First().Value) : 0);

            var model = new BaiTapViewModel
            {
                LopIdGoc = lopIdGoc,
                ClassIds = lopIdGoc > 0 ? new List<int> { lopIdGoc } : new List<int>(),
                AvailableClasses = availableClasses
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> TaoBaiTap(BaiTapViewModel model)
        {
            // Nếu chưa chọn lớp thì thêm lớp gốc vào để giữ nguyên checkbox
            model.ClassIds ??= new List<int>();
            if (model.LopIdGoc > 0 && !model.ClassIds.Contains(model.LopIdGoc))
            {
                model.ClassIds.Add(model.LopIdGoc);
            }

            // Kiểm tra hợp lệ
            if (!ModelState.IsValid || !model.ClassIds.Any())
            {
                model.AvailableClasses = _context.LopHocs
                    .Where(l => l.IsActive)
                    .Select(l => new SelectListItem
                    {
                        Value = l.Id.ToString(),
                        Text = l.Name,
                        Selected = model.ClassIds.Contains(l.Id)
                    }).ToList();

                ModelState.AddModelError("", "Vui lòng chọn ít nhất một lớp học.");
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin và chọn lớp học.";
                return View(model);
            }

            // Upload file nếu có
            string? fileUrl = null;
            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Attachment.CopyToAsync(stream);

                fileUrl = "/uploads/" + uniqueFileName;
            }

            // Lấy lớp gốc để redirect
            model.LopIdGoc = model.ClassIds.First();

            var baiTap = new BaiTap
            {
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                MaxPoint = model.MaxPoint ?? 100, // ✅ Gán mặc định nếu null
                CreatedDate = DateTime.Now,
                IsActive = true,
                ContentUrl = fileUrl,
                BaiTapLopHocs = model.ClassIds.Select(lopId => new BaiTapLopHoc
                {
                    LopHocId = lopId,
                    NgayGiao = DateTime.Now
                }).ToList()
            };

            _context.BaiTaps.Add(baiTap);
            await _context.SaveChangesAsync();

            // Gửi thông báo đến SV trong các lớp
            var thongBaos = new List<ThongBao>();
            foreach (var classId in model.ClassIds)
            {
                var sinhVienIds = await _context.SinhVienLopHocs
                    .Where(x => x.IdClass == classId)
                    .Join(_context.NguoiDungs,
                        sv => sv.IdSv,
                        nd => nd.Id,
                        (sv, nd) => new { sv, nd })
                    .Where(x => x.nd.Role == "Student")
                    .Select(x => x.sv.IdSv)
                    .ToListAsync();

                foreach (var svId in sinhVienIds)
                {
                    var tb = new ThongBao
                    {
                        NguoiNhanId = svId,
                        NoiDung = $"Bài tập mới \"{model.Title}\" đã được giao.",
                        LienKet = Url.Action("ChiTietBaiTap", "SinhVien", new { id = baiTap.Id }),
                        Loai = LoaiThongBao.BaiTapMoi,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    };

                    thongBaos.Add(tb);

                    await _hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                    {
                        tieuDe = $"Bài tập mới: {model.Title}",
                        link = tb.LienKet,
                        thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                    });
                }
            }

            _context.ThongBaos.AddRange(thongBaos);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Bài tập đã được tạo thành công.";
            return Redirect($"/Courses/DetailCourses/{model.LopIdGoc}#exerciseTab");
        }



        // GET: Sửa bài tập
        [HttpGet]
        public IActionResult SuaBaiTap(int id)
        {
            var baiTap = _context.BaiTaps
                .Include(bt => bt.BaiTapLopHocs)
                .FirstOrDefault(x => x.Id == id);

            if (baiTap == null)
                return NotFound();

            var model = new BaiTapViewModel
            {
                Title = baiTap.Title,
                Description = baiTap.Description,
                DueDate = baiTap.DueDate,
                ContentUrl = baiTap.ContentUrl,
                ClassIds = baiTap.BaiTapLopHocs?.Select(x => x.LopHocId).ToList() ?? new(),
                AvailableClasses = GetAvailableClasses()
            };

            ViewBag.Id = baiTap.Id;
            return View(model);
        }

        // POST: Cập nhật bài tập
        [HttpPost]
        public async Task<IActionResult> SuaBaiTap(int id, BaiTapViewModel model)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.BaiTapLopHocs)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (baiTap == null)
                return NotFound();
            var lopGoc = await _context.BaiTapLopHocs
    .Where(x => x.BaiTapId == id)
    .Select(x => x.LopHocId)
    .FirstOrDefaultAsync();

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                // Cập nhật file mới như bình thường
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Attachment.CopyToAsync(stream);

                baiTap.ContentUrl = "/uploads/" + uniqueFileName;
            }
            // else giữ nguyên file cũ, không gán gì hết


            // Cập nhật thông tin
            baiTap.Title = model.Title;
            baiTap.Description = model.Description;
            baiTap.DueDate = model.DueDate;

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.Attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Attachment.CopyToAsync(stream);

                baiTap.ContentUrl = "/uploads/" + uniqueFileName;
            }

            // Xóa liên kết cũ
            _context.BaiTapLopHocs.RemoveRange(baiTap.BaiTapLopHocs);

            // Thêm liên kết mới
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
            // Gửi thông báo đến các sinh viên trong các lớp được chọn
            var dsThongBao = new List<ThongBao>();

            foreach (var classId in model.ClassIds)
            {
                var sinhVienIds = await _context.SinhVienLopHocs
               .Where(x => x.IdClass == classId)
               .Join(_context.NguoiDungs,
                   sv => sv.IdSv,
                   nd => nd.Id,
                   (sv, nd) => new { sv, nd })
               .Where(x => x.nd.Role == "Student")
               .Select(x => x.sv.IdSv)
               .ToListAsync();


                foreach (var svId in sinhVienIds)
                {
                    var tb = new ThongBao
                    {
                        NguoiNhanId = svId,
                        NoiDung = $"Bài tập \"{model.Title}\" đã được cập nhật. Vui lòng xem lại nội dung mới.",
                        LienKet = Url.Action("ChiTietBaiTap", "SinhVien", new { id = baiTap.Id }),
                        Loai = LoaiThongBao.CapNhatBaiTap, // Nếu có enum
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    };

                    dsThongBao.Add(tb);

                    // Gửi realtime
                    await _hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                    {
                        tieuDe = $"Bài tập cập nhật: {model.Title}",
                        link = tb.LienKet,
                        thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                    });
                }
            }

            _context.ThongBaos.AddRange(dsThongBao);
            await _context.SaveChangesAsync();
            TempData["Success"] = " Bài tập đã được cập nhật thành công!";
            return Redirect($"/Courses/DetailCourses/{lopGoc}#exerciseTab");
        }


        // POST: Xóa bài tập
        [HttpPost]
        public async Task<IActionResult> XoaBaiTap(int id)
        {
            var baiTap = await _context.BaiTaps.FindAsync(id);
            if (baiTap == null)
                return NotFound();

            // Lấy lớp gốc để redirect và thông báo
            var lopGoc = await _context.BaiTapLopHocs
                .Where(x => x.BaiTapId == id)
                .Select(x => x.LopHocId)
                .FirstOrDefaultAsync();

            if (lopGoc == 0)
                return RedirectToAction("Index", "Courses");

            // Xóa các bản ghi nộp bài liên quan
            var nopBais = _context.NopBais.Where(n => n.TestId == id);
            _context.NopBais.RemoveRange(nopBais);

            // Xóa liên kết lớp học
            var baiTapLops = _context.BaiTapLopHocs.Where(x => x.BaiTapId == id);
            _context.BaiTapLopHocs.RemoveRange(baiTapLops);

            // Xóa bài tập
            _context.BaiTaps.Remove(baiTap);
            await _context.SaveChangesAsync(); // Lưu việc xóa trước


            var sinhVienIds = await _context.SinhVienLopHocs
       .Where(sv => sv.IdClass == lopGoc)
       .Join(_context.NguoiDungs,
           sv => sv.IdSv,
           nd => nd.Id,
           (sv, nd) => new { sv, nd })
       .Where(x => x.nd.Role == "Student")
       .Select(x => x.sv.IdSv)
       .Distinct()
       .ToListAsync();


            var danhSachThongBao = new List<ThongBao>();
            var lienKet = Url.Action("DetailCourses", "Courses", new { id = lopGoc }) + "#exerciseTab";
            var thoiGianNow = DateTime.Now;

            foreach (var svId in sinhVienIds)
            {
                danhSachThongBao.Add(new ThongBao
                {
                    NguoiNhanId = svId,
                    NoiDung = $"Bài tập \"{baiTap.Title}\" đã bị xóa.",
                    LienKet = Url.Action("DetailCourses", "Courses", new { id = lopGoc }) + "#exerciseTab",
                    Loai = LoaiThongBao.CapNhatBaiTap,
                    ThoiGian = thoiGianNow,
                    DaDoc = false
                });
            }

            _context.ThongBaos.AddRange(danhSachThongBao);
            await _context.SaveChangesAsync();

            // Gửi realtime đến từng sinh viên
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ThongBaoHub>>();
            foreach (var svId in sinhVienIds)
            {
                await hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                {
                    tieuDe = "Bài tập đã bị xóa",
                    link = lienKet,
                    thoiGian = thoiGianNow.ToString("HH:mm dd/MM")
                });
            }

            TempData["Success"] = "Bài tập đã được xóa thành công!";
            return Redirect($"/Courses/DetailCourses/{lopGoc}#exerciseTab");
        }





        private List<SelectListItem> GetAvailableClasses()
        {
            return _context.LopHocs.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.Name
            }).ToList();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> DetailCourses(int id, int page = 1)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var lop = await _context.LopHocs.FirstOrDefaultAsync(l => l.Id == id);
            if (lop == null)
                return NotFound();
            // Lấy danh sách sinh viên
            var students = await _context.SinhVienLopHocs
                .Include(x => x.IdSvNavigation)
                .Where(x => x.IdClass == id && x.IdSvNavigation.Role == "Student")
                .Select(x => x.IdSvNavigation)
                .ToListAsync();

            // Lấy danh sách giảng viên
            var teachers = await _context.GiangVienLopHocs
            .Include(x => x.IdGvNavigation)
            .Where(x => x.IdClass == id && x.IdGvNavigation.Role == "Teacher")
            .Select(x => x.IdGvNavigation)
            .ToListAsync();

            int pageSize = 6;

            var baiGiangsQuery = _context.LopHocBaiGiangs
                .Where(lbg => lbg.LopHocId == id)
                .Include(lbg => lbg.BaiGiang)
                .Select(lbg => lbg.BaiGiang)
                .OrderByDescending(b => b.CreatedDate);

            var paginatedBaiGiangs = await PhanTrang<BaiGiang>.CreateAsync(baiGiangsQuery, page, pageSize);

            var baiTapsQuery = _context.BaiTapLopHocs
                .Where(bt => bt.LopHocId == id)
                .Include(bt => bt.BaiTap)
                .Select(bt => bt.BaiTap)
                .OrderByDescending(bt => bt.CreatedDate);
            // Lấy danh sách bài giảng chưa được gán vào lớp này
            var allBaiGiangs = await _context.BaiGiangs.ToListAsync();

            var baiGiangDaCoIds = await _context.LopHocBaiGiangs
                .Where(lbg => lbg.LopHocId == id)
                .Select(lbg => lbg.BaiGiangId)
                .ToListAsync();

            var baiGiangsChuaCo = allBaiGiangs
                .Where(bg => !baiGiangDaCoIds.Contains(bg.Id))
                .OrderByDescending(bg => bg.CreatedDate)
                .ToList();


            var paginatedBaiTaps = await PhanTrang<BaiTap>.CreateAsync(baiTapsQuery, page, pageSize);

            var vm = new LopHocViewModel
            {
                Id = lop.Id,
                Name = lop.Name,
                Picture = lop.Picture,
                BaiGiangs = paginatedBaiGiangs,
                BaiTaps = paginatedBaiTaps,
                Students = students,
                Teachers = teachers,
                BaiGiangsChuaCo = baiGiangsChuaCo
            };

            ViewBag.LopId = id;
            return PartialView(vm);
        }
        [HttpPost]
        public async Task<IActionResult> ThemBaiGiangVaoLop(int lopHocId, List<int> selectedBaiGiangIds)
        {
            if (selectedBaiGiangIds == null || !selectedBaiGiangIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một bài giảng.";
                return Redirect($"/Courses/DetailCourses/{lopHocId}#contentTab");
            }

            var existingIds = await _context.LopHocBaiGiangs
                .Where(x => x.LopHocId == lopHocId && selectedBaiGiangIds.Contains(x.BaiGiangId))
                .Select(x => x.BaiGiangId)
                .ToListAsync();

            var newBaiGiangIds = selectedBaiGiangIds.Except(existingIds).ToList();

            foreach (var baiGiangId in newBaiGiangIds)
            {
                _context.LopHocBaiGiangs.Add(new LopHocBaiGiang
                {
                    LopHocId = lopHocId,
                    BaiGiangId = baiGiangId,
                    AddedDate = DateTime.Now
                });
            }

            if (newBaiGiangIds.Any())
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã thêm {newBaiGiangIds.Count} bài giảng vào lớp.";
            }
            else
            {
                TempData["Warning"] = "Các bài giảng đã được thêm trước đó.";
            }

            return Redirect($"/Courses/DetailCourses/{lopHocId}#contentTab");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Join(string code)
        {
            var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.JoinCode == code);
            if (lop == null)
                return NotFound();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Join", "Courses", new { code }) });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Không thể xác định tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            bool isJoined = await _context.SinhVienLopHocs.AnyAsync(x => x.IdClass == lop.Id && x.IdSv == userId);

            ViewBag.IsJoined = isJoined;
            return View(lop);
        }
        [AllowAnonymous]
        public async Task<IActionResult> JoinAsTeacher(string code)
        {
            var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.JoinCode == code);
            if (lop == null)
                return NotFound();

            // Nếu chưa đăng nhập → chuyển sang trang đăng nhập, quay lại sau
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("JoinAsTeacher", "Courses", new { code }) });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "Không thể xác định tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            // Chặn nếu user không phải là giảng viên
            if (role != "Teacher")
            {
                TempData["Error"] = "Bạn không có quyền tham gia lớp với vai trò giảng viên.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra giảng viên đã tham gia lớp chưa
            bool isJoined = await _context.GiangVienLopHocs.AnyAsync(x => x.IdClass == lop.Id && x.IdGv == userId);

            ViewBag.IsJoined = isJoined;
            return View(lop); // Tạo view riêng JoinAsTeacher.cshtml nếu muốn
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmJoin(int lopId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "Bạn cần đăng nhập để tham gia lớp.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ConfirmJoin", new { lopId }) });
            }

            var userId = int.Parse(userIdStr);

            var lop = await _context.LopHocs.FindAsync(lopId);
            if (lop == null)
            {
                TempData["Error"] = "Lớp học không tồn tại.";
                return RedirectToAction("Courses", "SinhVien");
            }

            var exists = await _context.SinhVienLopHocs.AnyAsync(x => x.IdClass == lopId && x.IdSv == userId);
            if (exists)
            {
                TempData["JoinSuccess"] = "Bạn đã tham gia lớp học này rồi.";
                return RedirectToAction("Courses", "SinhVien");
            }

            _context.SinhVienLopHocs.Add(new SinhVienLopHoc
            {
                IdClass = lopId,
                IdSv = userId,
                JoinDate = DateTime.Now,
                IsActive = true
            });

            var user = await _context.NguoiDungs.FindAsync(userId);
            var tenSinhVien = user?.Name ?? "Sinh viên";

            var giangVienId = await _context.GiangVienLopHocs
                .Where(gv => gv.IdClass == lopId && gv.IsActive)
                .Select(gv => gv.IdGv)
                .FirstOrDefaultAsync();

            if (giangVienId != 0)
            {
                var tb = new ThongBao
                {
                    NguoiNhanId = giangVienId,
                    NoiDung = $"{tenSinhVien} vừa tham gia lớp học.",
                    LienKet = Url.Action("DetailCourses", "Courses", new { id = lopId }) + "#peopleTab",
                    Loai = LoaiThongBao.ThamGiaLop,
                    ThoiGian = DateTime.Now,
                    DaDoc = false
                };
                _context.ThongBaos.Add(tb);

                var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ThongBaoHub>>();
                await hubContext.Clients.Group($"user_{giangVienId}").SendAsync("NhanThongBao", new
                {
                    tieuDe = $"{tenSinhVien} đã tham gia lớp",
                    link = tb.LienKet,
                    thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                });
            }

            await _context.SaveChangesAsync();
            TempData["JoinSuccess"] = "Bạn đã tham gia lớp học thành công!";
            return RedirectToAction("Courses", "SinhVien");
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmJoinAsTeacher(int lopId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "Bạn cần đăng nhập để tham gia lớp.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ConfirmJoinAsTeacher", new { lopId }) });
            }

            var userId = int.Parse(userIdStr);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Teacher")
            {
                TempData["Error"] = "Chỉ giảng viên mới có thể tham gia lớp với vai trò giảng viên.";
                return RedirectToAction("Index", "Home");
            }

            var lop = await _context.LopHocs.FindAsync(lopId);
            if (lop == null)
            {
                TempData["Error"] = "Lớp học không tồn tại.";
                return RedirectToAction("Courses", "GiangVien");
            }

            var exists = await _context.GiangVienLopHocs.AnyAsync(x => x.IdClass == lopId && x.IdGv == userId);
            if (exists)
            {
                TempData["JoinSuccess"] = "Bạn đã tham gia lớp học này rồi.";
                return RedirectToAction("Courses", "GiangVien");
            }

            _context.GiangVienLopHocs.Add(new GiangVienLopHoc
            {
                IdClass = lopId,
                IdGv = userId,
                AssignedDate = DateTime.Now,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            TempData["JoinSuccess"] = "Bạn đã tham gia lớp học với vai trò giảng viên!";
            return RedirectToAction("Courses", "GiangVien");
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> SendInvitation(InviteUsersViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Thông tin không hợp lệ.";
                return RedirectToAction("DetailCourses", new { id = model.ClassId });
            }

            var lop = await _context.LopHocs.FindAsync(model.ClassId);
            if (lop == null)
            {
                TempData["Error"] = "Không tìm thấy lớp học.";
                return RedirectToAction("DetailCourses", new { id = model.ClassId });
            }

            // Tùy thuộc vai trò, tạo link mời khác nhau
            var actionName = model.Role == "Teacher" ? "JoinAsTeacher" : "Join";
            var roleText = model.Role == "Teacher" ? "giảng viên" : "sinh viên";

            var confirmUrl = Url.Action(actionName, "Courses", new { code = lop.JoinCode }, Request.Scheme);

            var subject = $"📩 Mời bạn tham gia lớp học: {lop.Name}";
            var body = $@"
        Xin chào,<br/>
        Bạn được mời tham gia lớp học <strong>{lop.Name}</strong> với vai trò {roleText}.<br/>
        Nhấn vào liên kết bên dưới để xác nhận tham gia:<br/>
        <a href='{confirmUrl}'>{confirmUrl}</a><br/><br/>
        Trân trọng,<br/>
        Website Bài Giảng
    ";

            await _emailService.SendEmailAsync(model.Email, subject, body);

            TempData["Message"] = $"✅ Đã gửi lời mời {roleText} đến {model.Email}";
            return Redirect($"/Courses/DetailCourses/{model.ClassId}#peopleTab");
        }


        public IActionResult ChiTietBaiTapGV(int baiTapId)
        {
            var baiTap = _context.BaiTaps
                .Include(bt => bt.NopBais)
                    .ThenInclude(nb => nb.Users)
                .Include(bt => bt.BaiTapLopHocs)
                .FirstOrDefault(bt => bt.Id == baiTapId);

            if (baiTap == null)
                return NotFound();

            var viewModel = new ChamBaiTapViewModel
            {
                BaiTap = baiTap,
                DanhSachNop = baiTap.NopBais.ToList()
            };

            var baiTapLopHoc = baiTap.BaiTapLopHocs.FirstOrDefault();
            ViewBag.LopId = baiTapLopHoc?.LopHocId ?? 0;

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ChamDiem(int[] NopBaiIds, double?[] Points, string?[] FeedBacks, int lopId)
        {
            int length = NopBaiIds.Length;
            var dsThongBao = new List<ThongBao>();
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ThongBaoHub>>();

            for (int i = 0; i < length; i++)
            {
                var nopBai = _context.NopBais
                    .Include(nb => nb.Users)
                    .Include(nb => nb.Test)
                    .FirstOrDefault(nb => nb.Id == NopBaiIds[i]);

                if (nopBai != null)
                {
                    double? point = (i < Points.Length) ? Points[i] : null;
                    string? feedback = (i < FeedBacks.Length) ? FeedBacks[i] : null;

                    // Kiểm tra điểm hợp lệ
                    if (point is < 0 or > 100)
                    {
                        ModelState.AddModelError("", $"Điểm của bài nộp có ID {nopBai.Id} phải nằm trong khoảng 0 đến 100.");
                        TempData["Error"] = "❌ Có điểm không hợp lệ. Mỗi điểm phải nằm trong khoảng 0 đến 100.";
                        return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
                    }

                    nopBai.Point = point;
                    nopBai.FeedBack = feedback;

                    // Thông báo...
                    var tb = new ThongBao
                    {
                        NguoiNhanId = nopBai.UsersId,
                        NoiDung = $"Bạn đã được chấm điểm bài tập \"{nopBai.Test.Title}\" với số điểm {point?.ToString("0.##") ?? "?"}.",
                        LienKet = Url.Action("ChiTietBaiTap", "SinhVien", new { id = nopBai.TestId }),
                        Loai = LoaiThongBao.DaChamDiem,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    };

                    dsThongBao.Add(tb);

                    await hubContext.Clients.Group($"user_{nopBai.UsersId}").SendAsync("NhanThongBao", new
                    {
                        tieuDe = $"Bài tập \"{nopBai.Test.Title}\" đã được chấm điểm",
                        link = tb.LienKet,
                        thoiGian = tb.ThoiGian.ToString("HH:mm dd/MM")
                    });
                }
            }


            // Lưu thông báo
            _context.ThongBaos.AddRange(dsThongBao);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Đã chấm điểm và phản hồi thành công!";
            return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
        }
        [HttpGet]
        public async Task<IActionResult> TaiBangDiemExcel(int id)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.NopBais)
                    .ThenInclude(nb => nb.Users)
                .FirstOrDefaultAsync(bt => bt.Id == id);

            if (baiTap == null)
                return NotFound();

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("BangDiem");

            worksheet.Cells[1, 1].Value = "Họ tên";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "Điểm";
            worksheet.Cells[1, 4].Value = "Nhận xét";

            int row = 2;
            foreach (var nop in baiTap.NopBais)
            {
                worksheet.Cells[row, 1].Value = nop.Users?.Name ?? "(Không tên)";
                worksheet.Cells[row, 2].Value = nop.Users?.Email;
                worksheet.Cells[row, 3].Value = nop.Point ?? 0;
                worksheet.Cells[row, 4].Value = nop.FeedBack;
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"BangDiem_{baiTap.Title}_{DateTime.Now:yyyyMMddHHmm}.xlsx";

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }


    }
}
