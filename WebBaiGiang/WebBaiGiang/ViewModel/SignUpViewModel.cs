using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang.ViewModel
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên!")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [RegularExpression(@"^(?=.*[\p{L}])[\p{L}\p{M} \.'\-]+$", ErrorMessage = "Họ tên không được chứa ký tự đặc biệt hoặc để trống")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Định dạng email không đúng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không đúng")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
