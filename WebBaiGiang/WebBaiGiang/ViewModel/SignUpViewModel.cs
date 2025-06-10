using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage ="Vui lòng nhập Họ tên !")]
        public string Name { get; set; }

        [Required(ErrorMessage ="Vui lòng nhập email !")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu !")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không đúng")]
        public string ConfirmPassword { get; set; }

    }
}
