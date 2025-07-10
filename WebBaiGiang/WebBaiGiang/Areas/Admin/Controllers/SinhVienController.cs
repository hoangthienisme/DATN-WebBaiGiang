using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;
using WebBaiGiang.ViewModel;

namespace WebBaiGiang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SinhVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<SinhVienController> _logger;
        private readonly IEmailService _emailService;// tiêm dịch vụ gửi email
        public SinhVienController(WebBaiGiangContext context, ILogger<SinhVienController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {
            var sinhViens = await _context.NguoiDungs
                .Where(u => u.Role == "Student")
                .OrderByDescending(u => u.CreatedDate)
                .Select(u => new SinhVienVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return View(sinhViens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.Id == id && x.Role == "Student");

            if (user == null)
            {
                TempData["Error"] = "Sinh viên không tồn tại.";
                return RedirectToAction("Index");
            }

            // Đảo trạng thái
            user.IsActive = !user.IsActive;
            user.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();

            try
            {
                string subject, body;

                if (!user.IsActive)
                {
                    subject = "Tài khoản sinh viên đã bị khóa";
                    body = $@"Chào {user.Name},

                    Tài khoản sinh viên của bạn trên hệ thống bài giảng trực tuyến đã bị tạm khóa bởi quản trị viên.
                    
                    Nếu bạn cần hỗ trợ hoặc có thắc mắc, vui lòng liên hệ bộ phận quản trị.
                    
                    Trân trọng,
                    Ban quản trị hệ thống.";
                                    }
                                    else
                                    {
                                        subject = "Tài khoản sinh viên đã được mở khóa";
                                        body = $@"Chào {user.Name},
                    
                    Tài khoản sinh viên của bạn trên hệ thống bài giảng trực tuyến đã được mở khóa và có thể sử dụng lại.
                    
                    Nếu bạn có thắc mắc, vui lòng liên hệ bộ phận quản trị.
                    
                    Trân trọng,
                    Ban quản trị hệ thống.";
                }

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi email khi cập nhật trạng thái sinh viên {Email}", user.Email);
            }

            TempData["Success"] = "Cập nhật trạng thái tài khoản thành công.";
            return RedirectToAction("Index");
        }




    }
}
