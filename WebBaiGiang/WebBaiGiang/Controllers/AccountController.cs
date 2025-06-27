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
using WebBaiGiang.ViewModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Authentication.Google;
namespace WebBaiGiang.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;// tiêm dịch vụ gửi email
        public AccountController(WebBaiGiangContext context, ILogger<AccountController> logger, IEmailService emailService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _emailService = emailService;
        }
        // Đăng nhập bằng Google
        public IActionResult LoginWithGoogle()
        {
            _logger.LogInformation("🔵 Vào action LoginWithGoogle");
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        public async Task<IActionResult> GoogleResponse()
        {
            _logger.LogInformation("🔵 Vào action GoogleResponse");

            // Lấy thông tin từ Google (không phải từ cookie)
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (result?.Principal == null)
            {
                _logger.LogWarning("🔴 Đăng nhập Google thất bại (không có Principal)");
                TempData["Error"] = "Đăng nhập Google thất bại. Vui lòng thử lại.";
                return RedirectToAction("Login", "Account");
            }

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims
                .ToDictionary(c => c.Type, c => c.Value);

            claims.TryGetValue(ClaimTypes.Email, out var email);
            claims.TryGetValue(ClaimTypes.Name, out var name);

            _logger.LogInformation($"🟢 Google login thành công: {email}");

            // Kiểm tra user có tồn tại chưa
            var user = _context.NguoiDungs
                .Where(u => u.Email == email)
                .Select(u => new NguoiDung
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .FirstOrDefault();

            if (user == null)
            {
                // ✅ Tạo user mới
                user = new NguoiDung
                {
                    Name = name,
                    Email = email,
                    Password = null,
                    Role = "Student",
                    CreatedDate = DateTime.Now,
                    Avatar = "default_avatar.png"
                };

                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("🟢 Tạo mới user Google thành công");
                TempData["Success"] = "Chào mừng bạn! Tài khoản Google đã được đăng ký và đăng nhập thành công.";
            }
            else
            {
                TempData["Success"] = "Đăng nhập Google thành công!";
            }

            // Tạo claims để đăng nhập
            var identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(ClaimTypes.Role, user.Role ?? "Student")
    }, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Điều hướng theo vai trò
            return user.Role switch
            {
                "Admin" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                "Teacher" => RedirectToAction("Courses", "GiangVien"),
                _ => RedirectToAction("Courses", "SinhVien")
            };
        }


        // Đăng ký tài khoản
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existingUser = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            HttpContext.Session.SetString("Temp_Name", model.Name);
            HttpContext.Session.SetString("Temp_Email", model.Email);
            HttpContext.Session.SetString("Temp_Password", BCrypt.Net.BCrypt.HashPassword(model.Password));
            HttpContext.Session.SetString("Temp_Role", "Student");

            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);

            await _emailService.SendEmailAsync(model.Email, "Mã OTP xác nhận", $"Mã OTP của bạn là: {otp}");
            TempData["Success"] = "Gửi mã OTP thành công! Vui lòng kiểm tra email để xác thực.";
            return RedirectToAction("VerifyOtp");
        }
        // Xác thực OTP
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
            TempData["Error"] = " Mã OTP không đúng. Vui lòng kiểm tra lại.";
            return RedirectToAction("VerifyOtp");
        }
        // Đăng nhập
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(NguoiDung nguoiDung, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(nguoiDung.Email) || string.IsNullOrEmpty(nguoiDung.Password))
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(nguoiDung);
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == nguoiDung.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(nguoiDung.Password, user.Password))
            {
                ModelState.AddModelError("", " *Email hoặc mật khẩu không đúng.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(nguoiDung);
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", " *Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(nguoiDung);
            }


            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
         new Claim("Avatar", user.Avatar ?? "")
    };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);
            TempData["Success"] = " Đăng nhập thành công!";


            //  Nếu có returnUrl thì redirect về đó trước
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Nếu không có returnUrl thì chuyển theo Role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else if (user.Role == "Teacher")
            {
                return RedirectToAction("Courses", "GiangVien");
            }
            else if (user.Role == "Student")
            {
                return RedirectToAction("Courses", "SinhVien");
            }

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
        // đôỉ mật khẩu
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

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
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

            TempData["Success"] = " Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Home");
        }
        // Quên mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                TempData["Info"] = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu.";
                return RedirectToAction("ForgotPassword");
            }

            var resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(30);
            _context.SaveChanges();

            var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, protocol: Request.Scheme);
            var body = $"Bạn vui lòng nhấn vào link sau để đặt lại mật khẩu: <a href='{resetLink}'>Đặt lại mật khẩu</a>";

            _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", body);

            TempData["Success"] = " Đã gửi email đặt lại mật khẩu (nếu email tồn tại).";
            return RedirectToAction("ForgotPassword");
        }


        // Quên mật khẩu - gửi email
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = " Token không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }
        // Quên mật khẩu - gửi email
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
                TempData["Error"] = " Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return View(model);
            }

            // Băm mật khẩu mới rồi lưu
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Xóa token và thời hạn để không dùng lại
            user.ResetPasswordToken = null;
            user.ResetTokenExpiry = null;

            _context.SaveChanges();

            TempData["Success"] = " Đổi mật khẩu thành công! Bạn có thể đăng nhập lại.";
            return RedirectToAction("Login");
        }
        // Xem và cập nhật hồ sơ người dùng
        [HttpGet]
        public IActionResult Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem hồ sơ.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction("Login", "Account");
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(NguoiDung model, IFormFile? AvatarFile)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Error"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction("Login", "Account");
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Login", "Account");
            }

            // Cập nhật thông tin
            user.Name = model.Name;
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.UpdateDate = DateTime.Now;

            // ✅ Upload avatar nếu có
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/uploads");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid() + Path.GetExtension(AvatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }
                user.Avatar = "/img/uploads/" + fileName;
            }

            _context.SaveChanges();

            // ✅ Cập nhật lại claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Gender, user.Gender ?? ""),
        new Claim(ClaimTypes.Role, user.Role ?? "Student"),
        new Claim("Avatar", user.Avatar ?? "")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["Success"] = " Cập nhật thông tin thành công!";

            // Điều hướng lại theo vai trò
            return user.Role switch
            {
                "Teacher" => RedirectToAction("Courses", "GiangVien"),
                "Student" => RedirectToAction("Courses", "SinhVien"),
                "Admin" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                _ => RedirectToAction("Index", "Home")
            };
        }

    }
}
