using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
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
        public IActionResult TaoBaiTap(int lopId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }
            var availableClasses = _context.LopHocs
                .Where(l => l.IsActive && l.GiangVienLopHocs.Any(gv => gv.IdGv == userId))
                .Select(l => new SelectListItem
                {
                    Text = l.Name,
                    Value = l.Id.ToString()
                })
                .ToList();

            var model = new BaiTapViewModel
            {
                LopIdGoc = lopId,
                AvailableClasses = availableClasses
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
                MaxPoint = model.MaxPoint,
                CreatedDate = DateTime.Now,
                IsActive = true,
                ContentUrl = fileUrl,
                BaiTapLopHocs = model.ClassIds.Select(LopHocId => new BaiTapLopHoc
                {
                    LopHocId = LopHocId,
                    NgayGiao = DateTime.Now
                }).ToList()
            };

            _context.BaiTaps.Add(baiTap);
            await _context.SaveChangesAsync();

            return Redirect("/Courses/DetailCourses/" + model.LopIdGoc + "#exerciseTab");





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
        [AllowAnonymous]
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

            // Lấy các bài giảng liên kết với lớp học thông qua LopHocBaiGiang
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

            var paginatedBaiTaps = await PhanTrang<BaiTap>.CreateAsync(baiTapsQuery, page, pageSize);

            var vm = new LopHocViewModel
            {
                Id = lop.Id,
                Name = lop.Name,
                Picture = lop.Picture,
                BaiGiangs = paginatedBaiGiangs,
                BaiTaps = paginatedBaiTaps
            };

            return PartialView(vm);
        }

        [AllowAnonymous] 
        public async Task<IActionResult> Join(string code)
        {
            var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.JoinCode == code);
            if (lop == null) return NotFound();

            if (!User.Identity.IsAuthenticated)
            {
                // Chưa đăng nhập, chuyển đến trang đăng nhập + returnUrl
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

            var exists = await _context.SinhVienLopHocs
                .AnyAsync(x => x.IdClass == lopId && x.IdSv == userId);

            if (!exists)
            {
                _context.SinhVienLopHocs.Add(new SinhVienLopHoc
                {
                    IdClass = lopId,
                    IdSv = userId,
                    JoinDate = DateTime.Now,
                    IsActive = true
                });
                await _context.SaveChangesAsync();
                TempData["JoinSuccess"] = " Bạn đã tham gia lớp học thành công!";
            }

            return RedirectToAction("Courses", "SinhVien");
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> SendInvitation(InviteStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ, có thể xử lý lại hoặc thông báo
                TempData["Error"] = "Thông tin không hợp lệ.";
                return RedirectToAction("DetailCourses", new { id = model.ClassId });
            }

            // Lấy thông tin lớp học (nếu cần)
            var lop = await _context.LopHocs.FindAsync(model.ClassId);
            if (lop == null)
            {
                TempData["Error"] = "Không tìm thấy lớp học.";
                return RedirectToAction("DetailCourses", new { id = model.ClassId });
            }

            // Tạo mã tham gia và URL xác nhận
            var code = lop.JoinCode;
            var confirmUrl = Url.Action("Join", "Courses", new { code }, Request.Scheme);

            var subject = "📩 Mời bạn tham gia lớp học: " + lop.Name;
            var body = $"Xin chào,<br/><br/>Bạn được mời tham gia lớp học <strong>{lop.Name}</strong>.<br/>" +
                       $"Vui lòng nhấn vào liên kết bên dưới để xác nhận tham gia:<br/>" +
                       $"<a href='{confirmUrl}'>{confirmUrl}</a><br/><br/>" +
                       $"Thân ái,<br/>Website Bài Giảng";

            await SendEmail(model.Email, subject, body);

            TempData["Message"] = $"✅ Đã gửi lời mời đến {model.Email}";

            return Redirect("/Courses/DetailCourses/" + model.ClassId + "#peopleTab");

        }

        // Hàm gửi email 
        private async Task SendEmail(string toEmail, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(toEmail));
            message.From = new MailAddress("nguyenhoangthien120304@gmail.com", "Web Bài Giảng");
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("nguyenhoangthien120304@gmail.com", "xqfx kjxd tpmq fmju"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }


        public IActionResult ChiTietBaiTapGV(int baiTapId)
        {
            var baiTap = _context.BaiTaps
                .Include(bt => bt.NopBais)
                    .ThenInclude(nb => nb.Users)
                .Include(bt => bt.BaiTapLopHocs)
                .FirstOrDefault(bt => bt.Id == baiTapId);

            if (baiTap == null) return NotFound();

            var viewModel = new ChamBaiTapViewModel
            {
                BaiTap = baiTap,
                DanhSachNop = baiTap.NopBais.ToList()
            };

            // Gửi LopId cho view
            var baiTapLopHoc = baiTap.BaiTapLopHocs.FirstOrDefault();
            ViewBag.LopId = baiTapLopHoc?.LopHocId ?? 0;

            return View(viewModel); 
        }

        [HttpPost]
        public IActionResult ChamDiem(int[] NopBaiIds, double?[] Points, string?[] FeedBacks, int lopId)
        {
            for (int i = 0; i < NopBaiIds.Length; i++)
            {
                var nopBai = _context.NopBais.FirstOrDefault(nb => nb.Id == NopBaiIds[i]);
                if (nopBai != null)
                {
                    nopBai.Point = Points[i];
                    nopBai.FeedBack = FeedBacks[i];
                }
            }

            _context.SaveChanges();
            return Redirect($"/Courses/DetailCourses/{lopId}#exerciseTab");
        }


    }
}
