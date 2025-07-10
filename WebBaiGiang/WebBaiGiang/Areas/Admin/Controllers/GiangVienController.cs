using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Areas.Admin.Data;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GiangVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<GiangVienController> _logger;
        private readonly IEmailService _emailService;// tiêm dịch vụ gửi email
        public GiangVienController(WebBaiGiangContext context, ILogger<GiangVienController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // 📄 Hiển thị danh sách giảng viên
        public async Task<IActionResult> Index()
        {
            var giangViens = await _context.NguoiDungs
                                           .Where(u => u.Role == "Teacher")
                                           .OrderByDescending(u => u.CreatedDate)
                                           .ToListAsync();
            return View(giangViens);
        }
        [HttpGet]
        // ➕ Hiển thị form tạo giảng viên
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeacherViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra email tồn tại trong bảng NguoiDungs
            var exists = await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            // Tạo entity mới
            var user = new NguoiDung
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Role = "Teacher",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            // Mật khẩu mặc định là số điện thoại nếu có, nếu không thì dùng "123456"
            var defaultPassword = !string.IsNullOrEmpty(model.Phone) ? model.Phone : "123456";
            user.Password = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

            _context.Add(user);
            await _context.SaveChangesAsync();

            // Gửi email thông báo tài khoản
            try
            {
                            var subject = "Tài khoản giảng viên đã được tạo";
                            var body = $@"Chào {user.Name},
            
            Tài khoản giảng viên của bạn đã được tạo trên hệ thống.
            Thông tin đăng nhập:
            Email: {user.Email}
            Mật khẩu mặc định: {defaultPassword}
            
            Vui lòng đăng nhập và đổi mật khẩu ngay sau khi đăng nhập.
            
            Trân trọng,
            Ban quản trị hệ thống.";
            
                            await _emailService.SendEmailAsync(user.Email, subject, body);
                        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi email tạo tài khoản giảng viên cho {Email}", user.Email);
            }

            return RedirectToAction(nameof(Index));
        }




        // GET: Admin/GiangVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null || user.Role != "Teacher") return NotFound();

            var model = new EditGiangVienViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };

            return View(model);
        }

        // POST: Admin/GiangVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditGiangVienViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _context.NguoiDungs.FindAsync(id);
                if (user == null || user.Role != "Teacher") return NotFound();

                user.Name = model.Name;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.Id == id && x.Role == "Teacher");

            if (user == null)
            {
                TempData["Error"] = "Giảng viên không tồn tại.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            user.UpdateDate = DateTime.Now;
            await _context.SaveChangesAsync();

            // Gửi email thông báo
            try
            {
                string subject = user.IsActive
                    ? "Tài khoản giảng viên đã được mở khóa"
                    : "Tài khoản giảng viên đã bị khóa";

                string body = user.IsActive
                    ? $@"Chào {user.Name},
                
                Tài khoản giảng viên của bạn đã được mở khóa và có thể sử dụng lại hệ thống bình thường.
                
                Trân trọng,
                Ban quản trị hệ thống."
                                    : $@"Chào {user.Name},
                
                Tài khoản giảng viên của bạn đã bị tạm khóa và hiện không thể truy cập hệ thống.
                
                Nếu bạn có thắc mắc, vui lòng liên hệ ban quản trị.
                
                Trân trọng,
                Ban quản trị hệ thống.";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi email khi cập nhật trạng thái giảng viên: {Email}", user.Email);
            }

            TempData["Success"] = "Cập nhật trạng thái giảng viên thành công.";
            return RedirectToAction("Index");
        }





    }
}
