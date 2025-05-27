using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
namespace WebBaiGiang.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public AccountController(WebBaiGiangContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            // Lưu tạm dữ liệu đăng ký vào session (CHƯA LƯU DB)
            HttpContext.Session.SetString("Temp_Name", model.Name);
            HttpContext.Session.SetString("Temp_Email", model.Email);
            HttpContext.Session.SetString("Temp_Password", BCrypt.Net.BCrypt.HashPassword(model.Password));
            HttpContext.Session.SetString("Temp_Role", "Student");

            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);

            SendOtpEmail(model.Email, otp);

            return RedirectToAction("VerifyOtp");
        }

        private void SendOtpEmail(string email, string otp)
        {
            try
            {
                var fromAddress = new MailAddress("0306221375@caothang.edu.vn", "Web Bài Giảng");
                var toAddress = new MailAddress(email);
                const string fromPassword = "rpvj rrzt fuux uwel";
                const string subject = "Mã OTP xác nhận";
                string body = $"Mã OTP của bạn là: {otp}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 10000, // Rút ngắn timeout
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                };

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                // Ghi log hoặc tạm thời hiển thị lỗi để debug
                Console.WriteLine("Email gửi lỗi: " + ex.Message);
                throw; // hoặc bỏ throw nếu muốn tiếp tục chạy không gửi email
            }
        }

        public IActionResult VerifyOtp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otpInput)
        {
            var sessionOtp = HttpContext.Session.GetString("OTP");
            var email = HttpContext.Session.GetString("Temp_Email");

            if (sessionOtp == otpInput)
            {
                var user = new NguoiDung
                {
                    Name = HttpContext.Session.GetString("Temp_Name"),
                    Email = email,
                    Password = HttpContext.Session.GetString("Temp_Password"),
                    Role = HttpContext.Session.GetString("Temp_Role"),
                    CreatedDate = DateTime.Now,
                    Avatar = "default_avatar.png",
                    //IsEmailConfirmed = true
                };

                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();

                // Xóa session
                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("Temp_Name");
                HttpContext.Session.Remove("Temp_Email");
                HttpContext.Session.Remove("Temp_Password");
                HttpContext.Session.Remove("Temp_Role");

                TempData["Success"] = "Đăng ký và xác thực thành công!";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Mã OTP không đúng.";
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
