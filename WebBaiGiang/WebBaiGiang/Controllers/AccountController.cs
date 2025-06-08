using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang.Models;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

            // Lưu tạm dữ liệu 
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
                    Timeout = 10000, 
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
                Console.WriteLine("Email gửi lỗi: " + ex.Message);
                throw; 
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
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
         [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Email và mật khẩu là bắt buộc.");
            return View();
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }
     var claims = new List<Claim>
    {   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            if (user.Role == "Teacher")
            return RedirectToAction("Courses", "GiangVien");

        return RedirectToAction("Index", "Home");
    }

        public async Task<IActionResult> Logout()
        {
            // Xóa session
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("", "Xác nhận mật khẩu không khớp.");
                return View(model);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            TempData["Message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Home");
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress("0306221375@caothang.edu.vn", "Web Bài Giảng");
                var toAddress = new MailAddress(toEmail);
                const string fromPassword = "rpvj rrzt fuux uwel";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 10000,
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi mail: " + ex.Message);
                throw;
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                // Có thể không báo để tránh rò rỉ email tồn tại
                ModelState.AddModelError("", "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu.");
                return View();
            }
            var resetToken = Guid.NewGuid().ToString();

            user.ResetPasswordToken = resetToken;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(30);
            _context.SaveChanges();
            var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, protocol: HttpContext.Request.Scheme);
            SendEmail(user.Email, "Đặt lại mật khẩu", $"Bạn vui lòng nhấn vào link sau để đặt lại mật khẩu: {resetLink}");

            ViewBag.Message = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.NguoiDungs.FirstOrDefault(u =>
                u.ResetPasswordToken == model.Token &&
                u.ResetTokenExpiry > DateTime.Now);

            if (user == null)
            {
                ModelState.AddModelError("", "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            // Băm mật khẩu mới rồi lưu
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Xóa token và thời hạn để không dùng lại
            user.ResetPasswordToken = null;
            user.ResetTokenExpiry = null;

            _context.SaveChanges();

            ViewBag.Message = "Đổi mật khẩu thành công! Bạn có thể đăng nhập lại.";
            return View();
        }
    }
}
