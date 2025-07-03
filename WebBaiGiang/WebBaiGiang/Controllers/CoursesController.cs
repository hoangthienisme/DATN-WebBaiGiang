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
                .Select(l => new SelectListItem { Text = l.Name, Value = l.Id.ToString() })
                .ToList();

            var model = new BaiTapViewModel
            {
                LopIdGoc = lopId ?? 0, // hoặc null nếu kiểu là int?
                AvailableClasses = availableClasses
            };

            return View(model);
        }


        // POST: Lưu bài tập
        [HttpPost]
        public async Task<IActionResult> TaoBaiTap(BaiTapViewModel model)
        {
            if (!ModelState.IsValid || model.ClassIds == null || !model.ClassIds.Any())
            {
                model.AvailableClasses = _context.LopHocs
                    .Where(l => l.IsActive)
                    .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name })
                    .ToList();

                // Giữ lại lớp gốc nếu có
                model.ClassIds ??= new List<int>();
                if (model.LopIdGoc > 0 && !model.ClassIds.Contains(model.LopIdGoc))
                {
                    model.ClassIds.Add(model.LopIdGoc);
                }

                ModelState.AddModelError("", "Vui lòng chọn ít nhất một lớp học.");
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin và chọn lớp học.";
                return View(model);
            }

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

            // Đặt lớp gốc để redirect sau khi tạo
            model.LopIdGoc = model.ClassIds.First();

            var baiTap = new BaiTap
            {
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                MaxPoint = model.MaxPoint,
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
            // Gửi thông báo đến sinh viên các lớp
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
                    var noiDung = $"Bài tập mới \"{model.Title}\" đã được giao.";
                    var lienKet = Url.Action("ChiTietBaiTap", "SinhVien", new { id = baiTap.Id });

                    var tb = new ThongBao
                    {
                        NguoiNhanId = svId,
                        NoiDung = noiDung,
                        LienKet = lienKet,
                        Loai = LoaiThongBao.BaiTapMoi,
                        ThoiGian = DateTime.Now,
                        DaDoc = false
                    };

                    thongBaos.Add(tb);

                    // Gửi realtime qua SignalR
                    await _hubContext.Clients.Group($"user_{svId}").SendAsync("NhanThongBao", new
                    {
                        tieuDe = $"Bài tập mới: {model.Title}",
                        link = lienKet,
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

            if (!ModelState.IsValid || model.ClassIds == null || !model.ClassIds.Any())
            {
                model.AvailableClasses = GetAvailableClasses();
                ViewBag.Id = id;

                if (model.ClassIds == null || !model.ClassIds.Any())
                    ModelState.AddModelError("", "Vui lòng chọn ít nhất một lớp học.");

                TempData["Error"] = " Vui lòng kiểm tra lại thông tin trước khi cập nhật.";
                return View(model);
            }

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
            var lop = await _context.LopHocs.FirstOrDefaultAsync(l => l.Id == id);
            if (lop == null)
                          return NotFound();
            var students = await _context.SinhVienLopHocs
                .Where(x => x.IdClass == id && x.IdSvNavigation.Role == "Student")
                .Include(x => x.IdSvNavigation)
                .Select(x => x.IdSvNavigation)
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
                BaiGiangsChuaCo = baiGiangsChuaCo
            };


            return PartialView(vm);
        }
        [HttpPost]
        public async Task<IActionResult> ThemBaiGiangVaoLop(int lopHocId, int selectedBaiGiangId)
        {
            var exists = await _context.LopHocBaiGiangs
                .AnyAsync(x => x.LopHocId == lopHocId && x.BaiGiangId == selectedBaiGiangId);

            if (!exists)
            {
                _context.LopHocBaiGiangs.Add(new LopHocBaiGiang
                {
                    LopHocId = lopHocId,
                    BaiGiangId = selectedBaiGiangId,
                    AddedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
             TempData["Success"] = "Bài giảng đã được thêm thành công!";
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

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isJoined = await _context.SinhVienLopHocs.AnyAsync(x => x.IdClass == lop.Id && x.IdSv == userId);

            ViewBag.IsJoined = isJoined;
            return View(lop);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmJoin(int lopId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var exists = await _context.SinhVienLopHocs.AnyAsync(x => x.IdClass == lopId && x.IdSv == userId);
            if (!exists)
            {
                // Thêm sinh viên vào lớp
                _context.SinhVienLopHocs.Add(new SinhVienLopHoc
                {
                    IdClass = lopId,
                    IdSv = userId,
                    JoinDate = DateTime.Now,
                    IsActive = true
                });

                // Lấy tên người dùng
                var user = await _context.NguoiDungs.FindAsync(userId);
                var tenSinhVien = user?.Name ?? "Sinh viên";

                // Lấy giảng viên phụ trách lớp
                var giangVienId = await _context.GiangVienLopHocs
                    .Where(gv => gv.IdClass == lopId && gv.IsActive)
                    .Select(gv => gv.IdGv)
                    .FirstOrDefaultAsync();

                if (giangVienId != 0)
                {
                    // Tạo thông báo
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

                    // Gửi realtime
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
            }

            return RedirectToAction("Courses", "SinhVien");
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> SendInvitation(InviteStudentViewModel model)
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

            var confirmUrl = Url.Action("Join", "Courses", new { code = lop.JoinCode }, Request.Scheme);

            var subject = $"📩 Mời bạn tham gia lớp học: {lop.Name}";
            var body = $@"
                    Xin chào,<br/>
                    Bạn được mời tham gia lớp học <strong>{lop.Name}</strong>.<br/>
                    Vui lòng nhấn vào liên kết bên dưới để xác nhận tham gia:<br/>
                    <a href='{confirmUrl}'>{confirmUrl}</a><br/><br/>
                    Thân ái,<br/>
                    Website Bài Giảng
                ";


            await _emailService.SendEmailAsync(model.Email, subject, body);

            TempData["Message"] = $"✅ Đã gửi lời mời đến {model.Email}";
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

                    nopBai.Point = point;
                    nopBai.FeedBack = feedback;

                    // Tạo thông báo
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

                    // Gửi thông báo realtime
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
    }
}
